using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Cards;
using Game.Effects;
using Game.Menus;
using Game.Sleeves;
using Game.Territories;
using GreenOne.Console;
using System;
using System.IO;
using UnityEngine;

namespace Game.Console
{
    public class cmdAiTest : Command
    {
        const string ID = "aitest";
        const string DESC = "выполняет тестирование системы ИИ на виртуальных сражениях";

        static readonly string _persistentPath = Application.persistentDataPath;
        static bool _executing;
        static int _points;

        class PointsArg : CommandArg
        {
            const string ID = "points";
            const string DESC = "количество очков улучшений у каждой карты колоды (1-..)";

            public PointsArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                if (!int.TryParse(str, out int parse) || parse < 1)
                    return false;

                value = parse;
                return true;
            }
        }
        class CountArg : CommandArg
        {
            const string ID = "count";
            const string DESC = "количество повторений (1-1000)";

            public CountArg(Command command) : base(command, ValueType.Required, ID, DESC) { }
            public override bool TryParseValue(string str, out object value)
            {
                if (!base.TryParseValue(str, out value))
                    return false;
                if (!int.TryParse(str, out int parse) || parse < 1 || parse > 1000)
                    return false;

                value = parse;
                return true;
            }
        }

        class pFieldCard : BattleFieldCard, IBattleSleeveCard
        {
            public BattleSleeve Sleeve => Side.Sleeve;
            public ITableSleeveCard AsInSleeve => this;

            public pFieldCard(FieldCard data, BattleSide side) : base(data, side)
            {
                TryOnInstantiatedAction(GetType(), typeof(pFieldCard));
            }
            protected pFieldCard(pFieldCard src, BattleFieldCardCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pFieldCard));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleFieldCardCloneArgs cArgs)
                    return new pFieldCard(this, cArgs);
                else return null;
            }
            protected override async UniTask OnPostKilledBase_TOP(object sender, BattleKillAttemptArgs e)
            {
                await base.OnPostKilledBase_TOP(sender, e);
                pFieldCard card = (pFieldCard)sender;
                await card.Side.Ether.AdjustValue(1, null);
            }

            public bool CanTake()
            {
                return false;
            }
            public bool CanReturn()
            {
                return false;
            }
            public bool CanDropOn(TableField field)
            {
                if (field is BattleField bField)
                    return field.Card == null && bField.Side == Side;
                else return false;
            }

            public bool CanPullOut()
            {
                return Side.isMe && Side.CanAfford(this);
            }
            public bool CanPullIn()
            {
                return true;
            }

            public void OnDropOn(TableSleeveCardDropArgs e)
            {
                Side.Purchase(this);
                Side.Sleeve.Remove(this);
                Territory.PlaceFieldCard(this, (BattleField)e.field, Side);
            }
            public Tween OnPullOut(bool sleevePull)
            {
                return AsInSleeve.OnPullOutBase(sleevePull);
            }
            public Tween OnPullIn(bool sleevePull)
            {
                return AsInSleeve.OnPullInBase(sleevePull);
            }

            public void OnTake()
            {
            }
            public void OnReturn()
            {
            }
        }
        class pFloatCard : BattleFloatCard, IBattleSleeveCard
        {
            public BattleSleeve Sleeve => Side.Sleeve;
            public ITableSleeveCard AsInSleeve => this;

            public pFloatCard(FloatCard data, BattleSide side) : base(data, side)
            {
                TryOnInstantiatedAction(GetType(), typeof(pFloatCard));
            }
            protected pFloatCard(pFloatCard src, BattleFloatCardCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pFloatCard));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleFloatCardCloneArgs cArgs)
                    return new pFloatCard(this, cArgs);
                else return null;
            }

            public bool CanTake()
            {
                return false;
            }
            public bool CanReturn()
            {
                return false;
            }
            public bool CanDropOn(TableField field)
            {
                return Data.IsUsable(new TableFloatCardUseArgs(this, Territory));
            }

            public bool CanPullOut()
            {
                return Side.isMe && Side.CanAfford(this);
            }
            public bool CanPullIn()
            {
                return true;
            }

            public void OnTake()
            {
                AsInSleeve.OnTakeBase();
                Side.Territory.SetCardsColliders(false);
            }
            public void OnReturn()
            {
                AsInSleeve.OnReturnBase();
                Side.Territory.SetFieldsColliders(true);
            }
            public void OnDropOn(TableSleeveCardDropArgs e)
            {
                Side.Purchase(this);
                Side.Sleeve.Remove(this);
                Territory.PlaceFloatCard(this, Side);
            }
            public Tween OnPullOut(bool sleevePull)
            {
                return AsInSleeve.OnPullOutBase(sleevePull);
            }
            public Tween OnPullIn(bool sleevePull)
            {
                return AsInSleeve.OnPullInBase(sleevePull);
            }
        }
        class pSleeve : BattleSleeve
        {
            public pSleeve(BattleSide side, Transform parent) : base(side, parent)
            {
                TryOnInstantiatedAction(GetType(), typeof(pSleeve));
            }
            protected pSleeve(pSleeve src, BattleSleeveCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pSleeve));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleSleeveCloneArgs cArgs)
                    return new pSleeve(this, cArgs);
                else return null;
            }

            protected override ITableSleeveCard HoldingCardCreator(Card data)
            {
                IBattleSleeveCard card;
                if (data.isField)
                    card = new pFieldCard((FieldCard)data, Side);
                else card = new pFloatCard((FloatCard)data, Side);
                if (!card.Side.isMe)
                    card.Drawer?.FlipRendererY();
                return card;
            }
            protected override ITableSleeveCard HoldingCardCloner(ITableSleeveCard src, TableSleeveCloneArgs args)
            {
                BattleSleeveCloneArgs argsCast = (BattleSleeveCloneArgs)args;
                Card dataClone = (Card)src.Data.Clone();

                ITableSleeveCard cardClone;
                if (dataClone.isField)
                {
                    BattleFieldCardCloneArgs cardCArgs = new((FieldCard)dataClone, null, Side, argsCast.terrCArgs);
                    cardClone = (pFieldCard)src.Clone(cardCArgs);
                }
                else
                {
                    BattleFloatCardCloneArgs cardCArgs = new((FloatCard)dataClone, Side, argsCast.terrCArgs);
                    cardClone = (pFloatCard)src.Clone(cardCArgs);
                }
                return cardClone;
            }
        }
        class pSide : BattleSide
        {
            public pSide(BattleTerritory territory, bool isMe) : base(territory, isMe)
            {
                TryOnInstantiatedAction(GetType(), typeof(pSide));
            }
            protected pSide(pSide src, BattleSideCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pSide));
            }

            public override object Clone(CloneArgs args)
            {
                if (args is BattleSideCloneArgs cArgs)
                    return new pSide(this, cArgs);
                else return null;
            }

            protected override CardDeck DeckCreator()
            {
                int stage = _points switch
                {
                    >= 56 => 7,
                    >= 48 => 6,
                    >= 40 => 5,
                    >= 32 => 4,
                    >= 24 => 3,
                    >= 16 => 2,
                    _ => 1,
                };
                return new CardDeck(stage, _points);
            }
            protected override BattleSleeve SleeveCreator(Transform parent)
            {
                return new pSleeve(this, parent);
            }

            protected override int HealthAtStartFunc()
            {
                if (_points < 50)
                    return _points + 10;
                else return (int)Math.Ceiling(_points * Math.Log(_points));
            }
            protected override int GoldAtStartFunc()
            {
                return 1;
            }
            protected override int EtherAtStartFunc()
            {
                return 1;
            }
        }
        class pTerritory : BattleTerritory
        {
            public bool BattleStarted => PhasesPassed != 0;

            public pTerritory() : base(true, 5, null, false)
            {
                TryOnInstantiatedAction(GetType(), typeof(pTerritory));
            }
            protected pTerritory(pTerritory src, BattleTerritoryCloneArgs args) : base(src, args)
            {
                TryOnInstantiatedAction(GetType(), typeof(pTerritory));
            }
            public override object Clone(CloneArgs args)
            {
                if (args is BattleTerritoryCloneArgs cArgs)
                    return new pTerritory(this, cArgs);
                else return null;
            }

            protected override BattleSide PlayerSideCreator()
            {
                return new pSide(territory: this, isMe: true);
            }
            protected override BattleSide EnemySideCreator()
            {
                return new pSide(territory: this, isMe: false);
            }

            protected override BattleFieldCard FieldCardCreator(FieldCard data, BattleSide side)
            {
                return new pFieldCard(data, side);
            }
            protected override BattleFloatCard FloatCardCreator(FloatCard data, BattleSide side)
            {
                return new pFloatCard(data, side);
            }
        }

        public cmdAiTest() : base(ID, DESC) { }

        protected override void Execute(CommandArgInputDict args)
        {
            if (_executing)
            {
                TableConsole.Log("Команда находится в процессе выполнения.", LogType.Error);
                return;
            }
            _executing = true;
            int count = (int)args["count"].value;
            int points = (int)args["points"].value;
            _ = Loop(count, points);
        }
        protected override CommandArg[] ArgumentsCreator() => new CommandArg[]
        {
            new PointsArg(this),
            new CountArg(this),
        };

        async UniTask Loop(int count, int points)
        {
            _points = points;
            for (int i = 0; i < count; i++)
            {
                int j = 0;
                TableConsole.FileName = $"{ID}\\{i + 1}.log";
                pTerritory terr = new();
                terr.DestroyDrawer(true);
                terr.CreateFields();
                try
                {
                    await terr.Player.Sleeve.TakeMissingCards(true);
                    await terr.Enemy.Sleeve.TakeMissingCards(true);
                    await terr.NextPhase();
                    while (true)
                    {
                        if (j++ > 100)
                            throw new Exception("Turns limit reached.");
                        if (terr.PhaseSide == terr.Player)
                        {
                            await terr.Player.Gold.AdjustValue(1, null);
                            await terr.Player.ai.MakeTurn();
                        }
                        if (terr.IsConcluded) break;
                        if (terr.PhaseSide == terr.Enemy)
                        {
                            await terr.Enemy.Gold.AdjustValue(1, null);
                            await terr.Enemy.ai.MakeTurn();
                        }
                        if (terr.IsConcluded) break;
                    }
                }
                catch (Exception ex)
                {
                    TableConsole.Log($"Ошибка во время теста #{i + 1}.", LogType.Error);
                    string path = $"{_persistentPath}\\{ID}\\{i + 1}-error.log";
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllText(path, $"{ex}\n\n{ex.StackTrace}");
                }
                finally
                {
                    TableConsole.Log($"Тест #{i + 1} завершён.", LogType.Log);
                    terr.Dispose();
                }
            }
            TableConsole.Log("Тестирование завершено!", LogType.Log);
            TableConsole.FileName = "Console.log";
            _executing = false;
        }
    }
}
