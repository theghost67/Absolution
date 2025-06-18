using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Sleeves;
using Game.Territories;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tMilitaryService : PassiveTrait
    {
        const string ID = "military_service";
        private static TraitStatFormula _moxieF = new(false, 1, 0);
        private int2 _lastPos;

        public tMilitaryService() : base(ID)
        {
            name = Translator.GetString("trait_military_service_1");
            desc = Translator.GetString("trait_military_service_2");

            rarity = Rarity.Rare;
            tags = TraitTag.Static;
            range = BattleRange.none;

            frequency = 0;
        }
        protected tMilitaryService(tMilitaryService other) : base(other) { _lastPos = other._lastPos; }
        public override object Clone() => new tMilitaryService(this);

        protected override string DescContentsFormat(TraitDescriptiveArgs args)
        {
            return Translator.GetString("trait_military_service_3", _moxieF.Format(args.stacks)) + 
                   Translator.GetString("trait_military_service_4");

        }

        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        {
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;
            BattleFieldCard owner = trait.Owner;

            if (trait.WasAdded(e))
            {
                BattleField lastField = owner.Field;
                _lastPos = lastField.pos;
                await owner.TryAttachToField(null, trait);
                if (owner.Field != null || owner.IsKilled) return;
                await owner.Moxie.AdjustValue(-_moxieF.ValueInt(trait.GetStacks()), trait);
                if (owner.IsKilled) return;
                owner.Drawer?.transform.DOMove(lastField.Drawer.transform.position + Vector3.up * (owner.Side.isMe ? -30 : 30), 0.5f).SetEase(Ease.OutQuad);
                owner.Drawer?.DOFade(0, 0.5f);
                owner.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase);
                owner.Territory.AddToStash(owner);
            }
            else if (trait.WasRemoved(e))
            {
                owner.Drawer?.DOFade(1, 0.5f);
                BattleField lastField = owner.Territory.Field(_lastPos);
                await owner.TryAttachToField(lastField, trait);
                if (owner.Field != lastField)
                {
                    if (owner.Data.price.currency.id == "gold")
                         await owner.Side.Gold.AdjustValue(owner.Price, trait);
                    else await owner.Side.Ether.AdjustValue(owner.Price, trait);
                    owner.Side.Sleeve.Add((ITableSleeveCard)owner);
                }
                owner.Drawer?.priceIcon.RedrawValue();
                owner.Drawer?.moxieIcon.RedrawValue();
                owner.Territory.OnStartPhase.Remove(trait.GuidStr);
                owner.Territory.RemoveFromStash(owner);
            }
        }
        private async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInTerritory(territory);
            if (trait == null || trait.Owner == null || trait.Owner.IsKilled) return;
            await trait.SetStacks(0, trait);
        }
    }
}
