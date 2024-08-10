using Cysharp.Threading.Tasks;
using Game.Cards;
using Game.Territories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Traits
{
    /// <summary>
    /// Класс, представляющий один из игровых навыков.
    /// </summary>
    public class tSmellyTrapper : PassiveTrait
    {
        const string ID = "smelly_trapper";
        const string CARD_ID = "crap";
        const int PRIORITY = 3;
        static readonly TerritoryRange _range = TerritoryRange.ownerDouble;

        public tSmellyTrapper() : base(ID)
        {
            name = "Вонючий капканщик";
            desc = "Фу, убери говно!";

            rarity = Rarity.Rare;
            tags = TraitTag.None;
            range = BattleRange.none;
        }
        protected tSmellyTrapper(tSmellyTrapper other) : base(other) { }
        public override object Clone() => new tSmellyTrapper(this);

        public override string DescRich(ITableTrait trait)
        {
            string cardName = CardBrowser.GetCard(CARD_ID).name;
            return DescRichBase(trait, new TraitDescChunk[]
            {
                new($"В начале каждого хода (П{PRIORITY})",
                    $"Расставляет рядом с владельцем карты <i>{cardName}</i> с единицей здоровья. Тратит по заряду за каждую установленную карту."),
            });
        }
        public override float Points(FieldCard owner, int stacks)
        {
            return base.Points(owner, stacks) + PointsExponential(12, stacks, 4);
        }
        public override async UniTask OnStacksChanged(TableTraitStacksSetArgs e)
        { 
            await base.OnStacksChanged(e);
            if (!e.isInBattle) return;

            IBattleTrait trait = (IBattleTrait)e.trait;

            if (trait.WasAdded(e))
                trait.Territory.OnStartPhase.Add(trait.GuidStr, OnTerritoryStartPhase, PRIORITY);
            else if (trait.WasRemoved(e))
                trait.Territory.OnStartPhase.Remove(trait.GuidStr);
        }

        async UniTask OnTerritoryStartPhase(object sender, EventArgs e)
        {
            BattleTerritory territory = (BattleTerritory)sender;
            IBattleTrait trait = (IBattleTrait)TraitFinder.FindInBattle(territory);
            if (trait == null) return;

            BattleFieldCard owner = trait.Owner;
            if (owner.Field == null) return;

            BattleField[] fields = territory.Fields(owner.Field.pos, _range).WithoutCard().ToArray();
            if (fields.Length == 0) return;

            await trait.AnimActivation();
            foreach (BattleField field in fields)
            {
                FieldCard newCard = CardBrowser.NewField(CARD_ID);
                await trait.AdjustStacks(-1, trait);
                await territory.PlaceFieldCard(newCard, field, trait);
            }
        }
    }
}
