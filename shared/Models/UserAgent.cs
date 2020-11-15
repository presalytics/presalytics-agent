using System;

namespace shared.Models
{
    public class UserAgent
    {
        public Guid AgentId { get; set;}
        public int Priority { get; set;}
        public string Name { get; set;}
    }
}