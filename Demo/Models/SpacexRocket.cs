using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Demo.models
{
    public class SpacexRocket : SpacexRocketBase
    {
        public bool active { get; set; }

        public SpacexRocketLength? height { get; set; }

        public SpacexRocketLength? diameter { get; set; }

        public SpacexRocketMass? mass { get; set; }

        public List<SpacexRocketPayloadWeight>? payload_weights { get; set; }

        public SpacexRocketEngines? engines { get; set; }

        public string? rocket_name { get; set; }

        public class SpacexRocketLength
        {
            public float meters { get; set; }
        }

        public class SpacexRocketEngines
        {
            public float thrust_to_weight { get; set; }
        }

        public class SpacexRocketMass
        {
            public float kg { get; set; }
        }

        public class SpacexRocketPayloadWeight
        {
            public string? id { get; set; }

            public string? name { get; set; }

            public int kg { get; set; }

            public int lb { get; set; }
        }

        private string FormatNumber(float number) {
            return String.Format(CultureInfo.InvariantCulture,"{0:#,0.##}", number);
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"rocket name: {rocket_name}");
            sb.AppendLine($"active: {active}");
            sb.AppendLine($"height: {FormatNumber(height?.meters ?? 0)} meters");
            sb.AppendLine($"diameter: {FormatNumber(diameter?.meters ?? 0)} meters");
            sb.AppendLine($"mass: {FormatNumber(mass?.kg ?? 0)} kg");
            if (payload_weights != null)
                foreach (SpacexRocketPayloadWeight payloadWeight in payload_weights)
                    sb.AppendLine($"payload to {payloadWeight.name ?? ""}: {FormatNumber(payloadWeight.kg)} kg");
            sb.AppendLine($"Thrust to weight ratio: {engines?.thrust_to_weight ?? 0}");

            return sb.ToString();
        }

        public override bool Equals(object? obj) {
            return obj is SpacexRocket rocket &&
                   base.Equals(obj) &&
                   id == rocket.id &&
                   rocket_id == rocket.rocket_id &&
                   active == rocket.active &&
                   EqualityComparer<SpacexRocketLength?>.Default.Equals(height, rocket.height) &&
                   EqualityComparer<SpacexRocketLength?>.Default.Equals(diameter, rocket.diameter) &&
                   EqualityComparer<SpacexRocketMass?>.Default.Equals(mass, rocket.mass) &&
                   EqualityComparer<List<SpacexRocketPayloadWeight>?>.Default.Equals(payload_weights, rocket.payload_weights) &&
                   EqualityComparer<SpacexRocketEngines?>.Default.Equals(engines, rocket.engines) &&
                   rocket_name == rocket.rocket_name;
        }

        public override int GetHashCode() {
            HashCode hash = new HashCode();
            hash.Add(id);
            hash.Add(rocket_id);
            hash.Add(active);
            hash.Add(height);
            hash.Add(diameter);
            hash.Add(mass);
            hash.Add(payload_weights);
            hash.Add(engines);
            hash.Add(rocket_name);
            return hash.ToHashCode();
        }
    }
}
