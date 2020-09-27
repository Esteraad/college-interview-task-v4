using System;
using System.Collections.Generic;
using System.Text;

namespace college_interview_task_v4.Tests.models
{
    public class SpacexRocketBase
    {
        public int id { get; set; }

        public string rocket_id { get; set; }

        public override bool Equals(object? obj) {
            return obj is SpacexRocketBase @base &&
                   id == @base.id &&
                   rocket_id == @base.rocket_id;
        }

        public override string ToString() {
            return $"{id} {rocket_id}";
        }
    }
}
