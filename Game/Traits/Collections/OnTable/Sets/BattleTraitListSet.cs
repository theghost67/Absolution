using Game.Cards;
using System.Collections;
using System.Collections.Generic;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий набор списков пассивных и активных трейтов во время сражения (см. <see cref="IBattleTraitListElement"/>).
    /// </summary>
    public class BattleTraitListSet : TableTraitListSet, IEnumerable<IBattleTraitListElement>
    {
        public new BattleFieldCard Owner => _owner;
        public new BattlePassiveTraitList Passives => base.Passives as BattlePassiveTraitList;
        public new BattleActiveTraitList Actives => base.Actives as BattleActiveTraitList;
        readonly BattleFieldCard _owner;

        public BattleTraitListSet(BattleFieldCard owner) : base(owner)
        {
            _owner = owner;
            TryOnInstantiatedAction(GetType(), typeof(BattleTraitListSet));
        }
        public BattleTraitListSet(BattleTraitListSet src, BattleTraitListSetCloneArgs args) : base(src, args)
        {
            _owner = args.srcSetOwnerClone;
            TryOnInstantiatedAction(GetType(), typeof(BattleTraitListSet));
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleTraitListSetCloneArgs cArgs)
                 return new BattleTraitListSet(this, cArgs);
            else return null;
        }

        public new IEnumerator<IBattleTraitListElement> GetEnumerator()
        {
            foreach (BattlePassiveTraitListElement element in Passives)
                yield return element;
            foreach (BattleActiveTraitListElement element in Actives)
                yield return element;
        }

        // next methods invoke only once in ctor:
        protected override TablePassiveTraitList PassivesCreator()
        {
            return new BattlePassiveTraitList(this);
        }
        protected override TableActiveTraitList ActivesCreator()
        {
            return new BattleActiveTraitList(this);
        }

        protected override TablePassiveTraitList PassivesCloner(TablePassiveTraitList src, TableTraitListSetCloneArgs args)
        {
            BattleTraitListSetCloneArgs argsCast = (BattleTraitListSetCloneArgs)args;
            BattleTraitListCloneArgs listCArgs = new(this, argsCast.terrCArgs);
            return (BattlePassiveTraitList)src.Clone(listCArgs);
        }
        protected override TableActiveTraitList ActivesCloner(TableActiveTraitList src, TableTraitListSetCloneArgs args)
        {
            BattleTraitListSetCloneArgs argsCast = (BattleTraitListSetCloneArgs)args;
            BattleTraitListCloneArgs listCArgs = new(this, argsCast.terrCArgs);
            return (BattleActiveTraitList)src.Clone(listCArgs);
        }
    }
}
