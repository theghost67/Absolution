using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Effects;
using Game.Palette;
using Game.Territories;
using Game.Traits;
using GreenOne;
using MyBox;
using System;
using TMPro;
using UnityEngine;

namespace Game.Cards
{
    public class cUntilDawn : FloatCard
    {
        const string ID = "until_dawn";
        const string TRAIT_ID = "till_dawn";
        string _eventGuid;
        TableFinder _sideFinder;

        public cUntilDawn() : base(ID)
        {
            name = Translator.GetString("card_until_dawn_1");
            desc = Translator.GetString("card_until_dawn_2");

            rarity = Rarity.Rare;
            price = new CardPrice(CardBrowser.GetCurrency("ether"), 2);
        }
        protected cUntilDawn(cUntilDawn other) : base(other) { }
        public override object Clone() => new cUntilDawn(this);

        protected override string DescContentsFormat(CardDescriptiveArgs args)
        {
            string traitName = TraitBrowser.GetTrait(TRAIT_ID).name;
            return Translator.GetString("card_until_dawn_3", traitName);
        }
        public override DescLinkCollection DescLinks(CardDescriptiveArgs args)
        {
            return new DescLinkCollection()
            { new TraitDescriptiveArgs(TRAIT_ID) { linkFormat = true } };
        }
        public override bool IsUsable(TableFloatCardUseArgs e)
        {
            return e.isInBattle;
        }
        public override async UniTask OnUse(TableFloatCardUseArgs e)
        {
            
            if (!e.isInBattle) return;

            BattleFloatCard card = (BattleFloatCard)e.card;
            BattleSide side = card.Side;
            BattleTerritory terr = side.Territory;

            _eventGuid = card.GuidStr;
            _sideFinder = side.Finder;

            await terr.ContinuousAttachHandler_Add(_eventGuid, ContinuousAttach_Add);
            terr.OnStartPhase.Add(_eventGuid, OnStartPhase);
        }

        async UniTask OnStartPhase(object sender, EventArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            await terr.ContinuousAttachHandler_Remove(_eventGuid, ContinuousAttach_Remove);
            terr.OnStartPhase.Remove(_eventGuid);
            if (side.Drawer != null)
            {
                TextMeshPro text = VFX.CreateText(Translator.GetString("card_until_dawn_4"), ColorPalette.C1.ColorCur, Global.Root, 1.5f);
                Sequence seq = DOTween.Sequence(text);
                text.color = Color.white.WithAlpha(0);
                seq.Append(text.DOFade(1, 0.25f));
                seq.AppendInterval(0.75f);
                seq.Append(text.DOFade(0, 0.25f));
                seq.AppendCallback(text.Destroy);
                seq.Play();
            }
            await UniTask.Delay(1000);
            int health = (int)Mathf.Ceil(side.HealthAtStart * 0.5f);
            await side.Health.AdjustValue(health, null);
        }
        UniTask ContinuousAttach_Add(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            if (e.card.FirstFieldAttachment || e.source == null)
                return e.card.Traits.AdjustStacks(TRAIT_ID, 1, side);
            else return UniTask.CompletedTask;
        }
        UniTask ContinuousAttach_Remove(object sender, TableFieldAttachArgs e)
        {
            BattleTerritory terr = (BattleTerritory)sender;
            BattleSide side = (BattleSide)_sideFinder.FindInBattle(terr);
            if (e.card.FirstFieldAttachment || e.source == null)
                return e.card.Traits.AdjustStacks(TRAIT_ID, -1, side);
            else return UniTask.CompletedTask;
        }
    }
}
