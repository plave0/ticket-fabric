using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using AvailabilityTracker.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using TicketFabric.Entities;
using TicketFabric.Interfaces;
using TicketFabric.Exceptions;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Runtime.CompilerServices;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication;
using System.Text;
using System.Diagnostics;

namespace AvailabilityTracker
{

    [StatePersistence(StatePersistence.Persisted)]
    internal class AvailabilityTracker : Actor, IAvailabilityTracker, IRemindable
    {

        private const string availabilityCounterStateName = "availabilityCounter";

        public AvailabilityTracker(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
        }

        public async Task<UInt16> GetCount()
        {
            await Task.Delay(100);
            return await this.StateManager.GetStateAsync<UInt16>(availabilityCounterStateName);
        }

        public async Task SetCount(ushort amount)
        {
            await StateManager.SetStateAsync(availabilityCounterStateName, amount);
        }

        public async Task<string> Reserve(ushort amountToReserve)
        {
            var availabilityCounter = await this.GetCount();

            if (availabilityCounter >= amountToReserve)
            {
                await SetCount((ushort) (availabilityCounter - amountToReserve));

                string reservationKey = $"{Id.ToString()}-{DateTime.Now.ToString("yyMMddhhmmssfff")}";

                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
                    {
                        writer.Write(reservationKey);
                        writer.Write(amountToReserve);
                    }

                    await RegisterReminderAsync(
                        reservationKey,
                        ms.ToArray(),
                        TimeSpan.FromSeconds(20),
                        TimeSpan.MaxValue);
                }

                return reservationKey;
            }
            else
            {
                return "0";
            }
            
        }
        public async Task Buy(string reservationKey)
        {
            try
            {
                IActorReminder reservationExpirationReminder = GetReminder(reservationKey);
                await UnregisterReminderAsync(reservationExpirationReminder);
            }
            catch (ReminderNotFoundException) 
            {
                throw new ReservationTimeExeededException("Reservation expired");
            }
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {

            string reservationKey = "";
            ushort reservationAmount = 0;

            using (MemoryStream ms = new MemoryStream(state))
            {
                using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
                {
                    reservationKey = reader.ReadString();
                    reservationAmount = reader.ReadUInt16();
                }
            }

            IActorReminder reservationExpirationReminder = GetReminder(reservationKey);
            await UnregisterReminderAsync(reservationExpirationReminder);

            var currentState = await GetCount();
            await SetCount((ushort)(currentState + reservationAmount));

        }
    }
}
