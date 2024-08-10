using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tStalker : PassiveTrait
    {
        const string ID = "stalker";
        const int PRIORITY = 4;
        static readonly TraitStatFormula _strengthF = new(true, 0, 0.25f);

        public tStalker() : base(ID)
        {
            name = "Преследователь";
            desc = "...";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tStalker(tStalker other) : base(other) { }
        public override object Clone() => new tStalker(this);

        public override string DescRich(ITableTrait trait)
        {
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"После атаки владельца (П{PRIORITY})",
                    $"Если владелец не атаковал цель ранее, увеличивает свою силу на {_strengthF.Format(trait)}."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(40, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
        }

        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (trait.Storage.ContainsKey(e.Receiver.GuidStr)) return;

            trait.Storage.TryAdd(e.Receiver.GuidStr, null);
            await trait.AnimActivation();
            await owner.Strength.AdjustValueScale(_strengthF.Value(trait), trait);
        }
    }
}
