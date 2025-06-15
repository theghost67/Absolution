using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Sleeves;
using Game.Territories;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFurnitureProtection : ActiveTrait
    {
        const string ID = "furniture_protection";
        const string KEY = "sleeve";
        static readonly TraitStatFormula _moxieF = new(false, 2, 0);

        public tFurnitureProtection() : base(ID)
        {
            name = "Защита мебели";
            desc = "Кто съел мою еду в холодильнике? Кто сидел в моём кресле!? О, видимо, он ещё здесь, хе-хе.";

            rarity = Rarity.Epic;
            tags = TraitTag.Static;
            range = BattleRange.none;
        }
        protected tFurnitureProtection(tFurnitureProtection other) : base(other) { }
        public override object Clone() => new tFurnitureProtection(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            object mode = null;
            args.table?.Storage.TryGetValue(KEY, out mode);
            string str = $"<color>Перед совершением атаки на пустое союзное поле любой картой</color>\n" +
                         $"Владелец переместится на это поле прежде, чем карта совершит атаку.\n\n" +
                         $"<color>При активации на территории или в рукаве</color>\nПереключает режим в-рукаве, при включённом состоянии, " +
                         $"навык может активироваться даже если владелец находится в рукаве. При активации таким способом, понижает инициативу владельца на {_moxieF.Format(args.stacks, true)}.";
            if (mode != null)
            {
                string modeStr = (bool)mode ? "<color=green>ВКЛ</color>" : "<color=red>ВЫКЛ</color>";
                str += $" Режим в-рукаве: {modeStr}.";
            }
            return str;
        }
        public override BattleWeight WeightDeltaUseThreshold(BattleWeightResult<BattleActiveTrait> result)
        {
            return BattleWeight.One(result.Entity);
        }

        public override bool IsUsableInSleeve()
        {
            return true;
        }
        public override bool IsUsable(TableActiveTraitUseArgs e)
        {
            return base.IsUsable(e) && e.isInBattle;
        }
        protected override async UniTask OnUse(TableActiveTraitUseArgs e)
        {
            IBattleTrait trait = (IBattleTrait)e.trait;
            if (trait.Storage.ContainsKey(KEY))
                 trait.Storage.Remove(KEY);
            else trait.Storage.Add(KEY, null);
        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                await trait.Territory.ContinuousAttachHandler_Add(trait.GuidStr, ContinuousAttach_Add, trait.Owner);
            else if (trait.WasRemoved(e))
                await trait.Territory.ContinuousAttachHandler_Remove(trait.GuidStr, ContinuousAttach_Remove);
        }

        async UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null) return;
            card.OnInitiationPreSent.Add(trait.GuidStr, OnInitiationPreSent);
        }
        async UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            BattleFieldCard card = (BattleFieldCard)e.card;
            if (trait == null) return;
            card.OnInitiationPreSent.Remove(trait.GuidStr);
        }

        async UniTask OnInitiationPreSent(object sender, BattleInitiationSendArgs e)
        {
            BattleFieldCard attacker = (BattleFieldCard)sender;
            BattleTerritory terr = attacker.Territory;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(terr);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled) return;

            bool fromSleeve = trait.Side.Sleeve.Contains(trait.Owner as ITableSleeveCard);
            if (!trait.Storage.ContainsKey(KEY) && fromSleeve) return;

            BattleField field = e.Receivers.FirstOrDefault(f => f.Side == trait.Side && f.Card == null);
            if (field == null) return;

            await trait.AnimActivation();
            if (fromSleeve)
                trait.Side.Sleeve.Remove(trait.Owner as ITableSleeveCard);
            await trait.Owner.TryAttachToField(field, trait);
            if (fromSleeve)
                await trait.Owner.Moxie.AdjustValue(-_moxieF.ValueInt(trait.GetStacks()), trait);
        }
    }
}
