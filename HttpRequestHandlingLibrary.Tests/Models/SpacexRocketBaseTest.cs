
using System;

namespace college_interview_task_v4.Tests.models
{
    public class SpacexRocketBaseTest
    {
        public int id { get; set; }

        public string rocket_id { get; set; }

        public override bool Equals(object? obj) {
            return obj is SpacexRocketBaseTest @base &&
                   id == @base.id &&
                   rocket_id == @base.rocket_id;
        }

        public override int GetHashCode() {
            return HashCode.Combine(id, rocket_id);
        }
    }
}
