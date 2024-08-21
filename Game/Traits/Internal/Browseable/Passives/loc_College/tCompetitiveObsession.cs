using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Menus;
using Game.Territories;
using System;

namespace Game.Traits
{
    // TODO: rework this trait before non-demo version release
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCompetitiveObsession : PassiveTrait
    {
        const string ID = "competitive_obsession";
        const int PRIORITY = 5;
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.10f);
        //static readonly TraitStatFormula _healthF = new(true, 0, 0.3333f);
        const string STRENGTH_STORAGE_ID = "strength";
        //const string HEALTH_STORAGE_ID = "health";

        public tCompetitiveObsession() : base(ID)
        {
            name = "Соревновательная одержимость";
            desc = "Раз на раз в кс пойдём, выйдем.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tCompetitiveObsession(tCompetitiveObsession other) : base(other) { }
        public override object Clone() => new tCompetitiveObsession(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>После победы в сражении (П{PRIORITY})</color>\nНавсегда увеличивает силу на {_strengthF.Format(args.stacks, true)}.";
            //return $"<color>После победы в сражении (П{PRIORITY})</color>\nНавсегда увеличивает силу на {_strengthF.Format(args.stacks, true)}.\n\n" +
            //       $"<color>После поражения в сражении (П{PRIORITY})</color>\nНавсегда уменьшает здоровье на {_healthF.Format(args.stacks, true)}.";
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(24, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);

            if (e.trait.Data.storage.TryGetValue(STRENGTH_STORAGE_ID, out object strengthRel))
                await e.trait.Owner.Strength.AdjustValueScale((float)strengthRel, e.trait);

            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;
            Trait traitData = trait.Data;
            TraitStorage traitStorage = traitData.storage;

            if (!trait.Side.isMe)
            {
                traitStorage[STRENGTH_STORAGE_ID] = UnityEngine.Random.Range(0, ((BattlePlaceMenu)Menu.GetCurrent()).DemoDifficulty) * 0.1f; // TODO: set to 20 in non-demo
                return;
            }
            if (trait.WasAdded(e))
            {
                owner.Territory.OnPlayerWon.Add(trait.GuidStr, OnPlayerWon, PRIORITY);
                //owner.Territory.OnPlayerLost.Add(trait.GuidStr, OnPlayerLost, PRIORITY);
            }
            else if (trait.WasRemoved(e))
            {
                owner.Territory.OnPlayerWon.Remove(trait.GuidStr);
                //owner.Territory.OnPlayerLost.Remove(trait.GuidStr);
            }
        }

        async UniTask OnPlayerWon(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null) return;

            Trait data = trait.Data;
            TraitStorage traitStorage = data.storage;
            if (traitStorage.TryGetValue(STRENGTH_STORAGE_ID, out object value))
                 traitStorage[STRENGTH_STORAGE_ID] = ((float)value) + _strengthF.valuePerStack;
            else traitStorage[STRENGTH_STORAGE_ID] = _strengthF.valuePerStack;
        }
        //async UniTask OnPlayerLost(object sender, EventArgs e)
        //{
        //    BattleTerritory terr = (BattleTerritory)sender;
        //    IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
        //    if (trait == null) return;

        //    Trait data = trait.Data;
        //    TraitStorage traitStorage = data.storage;
        //    if (traitStorage.TryGetValue(HEALTH_STORAGE_ID, out object value))
        //         traitStorage[HEALTH_STORAGE_ID] = ((float)value) - _healthF.valuePerStack;
        //    else traitStorage[HEALTH_STORAGE_ID] = -_healthF.valuePerStack;
        //}
    }
}
