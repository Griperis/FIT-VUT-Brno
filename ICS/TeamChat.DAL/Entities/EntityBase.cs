using System;
using TeamChat.DAL.Interfaces;

namespace TeamChat.DAL.Entities
{
    public abstract class EntityBase : IEntity
    {
         public Guid Id { get; set; }
    }
}
