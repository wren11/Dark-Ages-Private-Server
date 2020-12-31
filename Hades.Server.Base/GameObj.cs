using System;
using System.Dynamic;
using System.Reflection.Metadata.Ecma335;

namespace Hades.Server.Base
{

    public interface IActor<in TObject, in TActor>
    {
        void Clicked(TObject actor, TActor obj);
    }

    public interface IReactor<out TObject, in TActor>
    {
        TObject Update(TActor actor, TimeSpan elapsedTime);
    }


    public interface IActingPlayer : IActor<Player, Creature>
    {

    }

    public interface IActingCreature : IActor<Creature, Player>, IReactor<Player, Creature>
    {

    }


    public class Player : IGameObj<Player>, IActingPlayer
    {

        public void Clicked(Player actor, Creature obj)
        {
            actor.Actor(actor, obj);
        }

        public IGameObj<TObject> Actor<TObject>(Player obj, TObject actor)
        {
            Console.WriteLine("Player Is Reacting to Object -> Creature Actor");

            return obj.Actor<TObject>(this, actor);
        }
    }

    public class Creature : IGameObj<Creature>, IActingCreature
    {
        
        public void Clicked(Creature actor, Player obj)
        {
            actor.Actor(actor, obj);
        }

        public Player Update(Creature actor, TimeSpan elapsedTime)
        {
            return null;
        }

        public IGameObj<TObject> Actor<TObject>(Creature obj, TObject actor)
        {
            Console.WriteLine("Creature Is Reacting to Object -> Player Actor");

            return obj.Actor<TObject>(this, actor);
        }
    }

    public interface IGameObj<in T>
    {
        IGameObj<TObject> Actor<TObject>(T obj, TObject actor);
    }

}

