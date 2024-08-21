using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tStealth : PassiveTrait
    {
        const string ID = "stealth";
        const int PRIORITY = 7;
        const string TRAIT_ID = "evasion";
        static readonly TraitStatFormula _stacksF = new(false, 0, 1);

        public tStealth() : base(ID)
        {
            name = "Стелс";
            desc = "Стелс.";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tStealth(tStealth other) : base(other) { }
        public override object Clone() => new tStealth(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>После убийства любой карты владельцем с одной атаки (П{PRIORITY})</color>\n" +
                   $"накладывает на владельца навык <u>{traitName}</u> с {_stacksF.Format(args.stacks)} зарядов.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true, stacks = _stacksF.ValueInt(args.stacks) } };
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(32, stacks);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
            {
                trait.Owner.OnInitiationConfirmed.Add(trait.GuidStr, OnOwnerInitiationConfirmed, PRIORITY);
                trait.Owner.OnKillConfirmed.Add(trait.GuidStr, OnOwnerKillConfirmed, PRIORITY);
            }
            else if (trait.WasRemoved(e))
            {
                trait.Owner.OnInitiationConfirmed.Remove(trait.GuidStr);
                trait.Owner.OnKillConfirmed.Remove(trait.GuidStr);
            }
        }

        static async UniTask OnOwnerInitiationConfirmed(object sender, BattleInitiationRecvArgs e)
        {
            BattleFieldCard receiver = e.Receiver.Card;
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (receiver == null) return;
            trait.Storage.TryAdd(receiver.GuidStr, null);
        }
        static async UniTask OnOwnerKillConfirmed(object sender, BattleKillConfirmArgs e)
        {
            BattleFieldCard owner = (BattleFieldCard)sender;
            IBattleTrait trait = owner.Traits.Any(ID);
            if (trait == null) return;
            if (trait.Storage.ContainsKey(e.victim.GuidStr)) return;
            await trait.AnimActivation();
            await owner.Traits.AdjustStacks(TRAIT_ID, trait.GetStacks(), trait);
        }
    }
}
