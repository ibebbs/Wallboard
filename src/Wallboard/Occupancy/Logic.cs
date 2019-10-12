using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Linq;

namespace Wallboard.Occupancy
{
    public static class Logic
    {
        public static IObservable<State> WhenOccupancyChanges(IObservable<Message> messages)
        {
            // The is a sensor on the door which reports when the door is opened or closed
            // Use this to turn the wall board on
            var turnOnWhenDoorOpens = messages
                .Where(message => message.Device == "MCCGQ01LM_office")
                .Select(_ => State.Present);

            // There is a RTCGQ01LM above the door, it's fairly flakey but would report presence
            // faster than the shelf Occupancy Sensor
            var turnOnWhenDoorPresenceSensorReportsPresence = messages
                .Where(message => message.Device == "RTCGQ01LM_office" && message.Payload.Contains("\"occupancy\":true"))
                .Select(_ => State.Present);

            /// There is a RTCGQ11LM on the shelf opposite my desk. Should continue to report
            /// presence even when I'm "in the flow".
            var turnOnWhenShelfPresenceSensorReportsPresence = messages
                .Where(message => message.Device == "RTCGQ11M_office_shelves" && message.Payload.Contains("\"occupancy\":true"))
                .Select(_ => State.Present);

            /// There is a RTCGQ11LM on the shelf opposite my desk. Should pretty reliably report
            /// abscense
            var turnOffwhenShelfPresenceSensorReportsAbscence = messages
                .Where(message => message.Device == "RTCGQ11M_office_shelves" && message.Payload.Contains("\"occupancy\":false"))
                .Select(_ => State.Abscent);

            // Get illuminance from any of the sensors
            var illuminance = messages
                .Where(message => message.Device.StartsWith("RTCGQ11M", StringComparison.OrdinalIgnoreCase))
                .Select(Presence.Message.Create)
                .Select(presence => presence.Illuminance);

            return Observable
                // Merge all the state observables ...
                .Merge(
                    turnOnWhenDoorOpens,
                    turnOnWhenDoorPresenceSensorReportsPresence,
                    turnOnWhenShelfPresenceSensorReportsPresence,
                    turnOffwhenShelfPresenceSensorReportsAbscence)
                // ... but only allow a State.Present to propagate when illuminance is above 50 ...
                .WithLatestFrom(illuminance, (s, i) => i > 50 ? s : State.Abscent)
                // ... debounce to prevent rapid changes ...
                .Throttle(TimeSpan.FromSeconds(10))
                // ... and prevent duplicates status' from being emitted ...
                .DistinctUntilChanged();
        }
    }
}
