using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tCamper : PassiveTrait
    {
        const string ID = "camper";
        const string TRAIT_ID = "trauma";

        public tCamper() : base(ID)
        {
            name = "Кэмпер";
            desc = "А прикинь он щас *вжух* и исчезнет?";

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.oppositeTriple);
        }
        protected tCamper(tCamper other) : base(other) { }
        public override object Clone() => new tCamper(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return $"<color>Пока присутствует на территории</color>\nРядомстоящие вражеские карты временно получают навык <nobr><u>{traitName}</u></nobr>.";
        }
        public override DescLinkCollection DescLinks(TraitDescriptiveArgs args)
        {
            return new DescLinkCollection() { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard target = e.target;

            if (target.IsKilled) return;
            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(target);
                await target.Traits.Passives.AdjustStacks(TRAIT_ID, 1, trait);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(target);
                await target.Traits.Passives.AdjustStacks(TRAIT_ID, -1, trait);
            }
        }
    }
}
