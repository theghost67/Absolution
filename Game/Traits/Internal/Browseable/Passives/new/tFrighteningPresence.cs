using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tFrighteningPresence : PassiveTrait
    {
        const string ID = "frightening_presence";
        static readonly TraitStatFormula _moxieF = new(false, 1, 0);

        public tFrighteningPresence() : base(ID)
        {
            name = "Пугающее присутствие";
            desc = "Мне страшно стоять рядом с ним...";

            rarity = Rarity.None;
            tags = TraitTag.Static;
            range = new BattleRange(TerritoryRange.ownerRadiusSmall);

            frequency = 0.5f;
        }
        protected tFrighteningPresence(tFrighteningPresence other) : base(other) { }
        public override object Clone() => new tFrighteningPresence(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return $"<color>Пока присутствует на территории</color>\nВременно понижает инициативу всех рядомстояющих карт на {_moxieF.Format(args.stacks, true)}.";
        }

        public override async UniTask OnTargetStateChanged(BattleTraitTargetStateChangeArgs e)
        {
            await base.OnTargetStateChanged(e);
            IBattleTrait trait = e.trait;
            BattleFieldCard target = e.target;
            string guid = trait.GuidGen(target.Guid);
            int moxie = -_moxieF.ValueInt(trait.GetStacks());

            if (e.canSeeTarget)
            {
                await trait.AnimDetectionOnSeen(target);
                await target.Moxie.AdjustValue(moxie, trait, guid);
            }
            else
            {
                await trait.AnimDetectionOnUnseen(target);
                await target.Moxie.RevertValue(guid);
            }
        }
    }
}
