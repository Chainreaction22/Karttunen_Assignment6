using System;

namespace Homma
{
    public class Log
    {
        public Guid Id { get; set; }
        public string Description { get; set; }

        public Log () {
            Id = Guid.NewGuid();
        }
    }
}