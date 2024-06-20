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
        public new BattlePassiveTraitList Passives => _passives;
        public new BattleActiveTraitList Actives => _actives;

        readonly BattleFieldCard _owner;
        BattlePassiveTraitList _passives;
        BattleActiveTraitList _actives;

        public BattleTraitListSet(BattleFieldCard owner) : base(owner)
        {
            _owner = owner;
            _passives = (BattlePassiveTraitList)base.Passives;
            _actives = (BattleActiveTraitList)base.Actives;
        }
        public BattleTraitListSet(BattleTraitListSet src, BattleTraitListSetCloneArgs args) : base(src, args)
        {
            _owner = args.srcSetOwnerClone;
            args.AddOnClonedAction(src.GetType(), typeof(BattleTraitListSet), () =>
            {
                _passives = (BattlePassiveTraitList)base.Passives;
                _actives = (BattleActiveTraitList)base.Actives;
            });
        }

        public override object Clone(CloneArgs args)
        {
            if (args is BattleTraitListSetCloneArgs cArgs)
                 return new BattleTraitListSet(this, cArgs);
            else return null;
        }

        public new IEnumerator<IBattleTraitListElement> GetEnumerator()
        {
            foreach (BattlePassiveTraitListElement element in _passives)
                yield return element;
            foreach (BattleActiveTraitListElement element in _actives)
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
