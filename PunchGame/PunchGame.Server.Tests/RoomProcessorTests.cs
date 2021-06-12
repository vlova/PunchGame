using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Models;
using PunchGame.Server.Room.Core.Output;
using System;
using System.Collections.Generic;

namespace PunchGame.Server.Tests
{
    public class RoomProcessorTests
    {
        [Test]
        [Category("RoomProcessor")]
        [TestCaseSource(nameof(TestCasesForNUnit))]
        public void WorksAsExpected(TestCase testCase)
        {
            var room = new RoomProcessor(new StubRandomProvider(testCase.RandomValue), testCase.Config);
            var initialRoomState = room.MakeInitialState();

            var (afterPrepareRoomState, _) = room.Process(
               initialRoomState,
               testCase.PrepareCommands);

            var (_, producedEvents) = room.Process(
               initialRoomState,
               testCase.PrepareCommands);

            Assert.That(producedEvents,
                new CollectionEquivalentConstraint(testCase.ExpectedEvents).Using(MakeJsonValueComparer()));
        }

        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                var initialLifeAmount = 100;

                var roomConfig = new RoomConfig
                {
                    Player = new PlayerConfig
                    {
                        InitialLifeAmount = initialLifeAmount,
                        Punch = new PunchConfig
                        {
                            Damage = 5,
                            CriticalChance = 0.5m,
                            CriticalDamage = 50,
                            MinimalTimeDiff = TimeSpan.FromSeconds(1)
                        }
                    },
                    TimeQuant = TimeSpan.FromSeconds(0.1)
                };

                // single player join
                {
                    var playerConnectionId = Guid.NewGuid();
                    var playerName = "trololo";
                    var timestamp = DateTime.UtcNow;
                    var playerId = Guid.NewGuid();

                    yield return new TestCase
                    {
                        CaseName = "SinglePlayerJoin",
                        Config = roomConfig,
                        PlayerIds = {
                            playerId,
                        },
                        ActCommands =
                        {
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = playerConnectionId,
                                Name = playerName,
                                Timestamp = timestamp
                            }
                        },
                        ExpectedEvents =
                        {
                            new AttemptToJoinSuccessfulEvent {
                                ConnectionId = playerConnectionId,
                                JoinedAsPlayerId = playerId,
                                Players = new List<AttemptToJoinSuccessfulEvent.ShortPlayerInfo> {
                                    new AttemptToJoinSuccessfulEvent.ShortPlayerInfo
                                    {
                                        IsConnected = true,
                                        Life = initialLifeAmount,
                                        UserId = playerId,
                                        Name = playerName
                                    }
                                },
                                Timestamp = timestamp,
                            },
                            new PlayerJoinedEvent
                            {
                                LifeAmount = initialLifeAmount,
                                Name = playerName,
                                PlayerId = playerId,
                                Timestamp = timestamp
                            }
                        }
                    };
                }

                // join of second player 
                {
                    var firstPlayer = new
                    {
                        ConnectionId = Guid.NewGuid(),
                        Id = Guid.NewGuid(),
                        Name = "first",
                    };

                    var secondPlayer = new
                    {
                        ConnectionId = Guid.NewGuid(),
                        Id = Guid.NewGuid(),
                        Name = "second",
                    };

                    var timestamp = DateTime.UtcNow;

                    yield return new TestCase
                    {
                        CaseName = "JoinOfSecondPlayerStartsGame",
                        Config = roomConfig,
                        PlayerIds = {
                            firstPlayer.Id,
                            secondPlayer.Id,
                        },
                        PrepareCommands =
                        {
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Name = firstPlayer.Name,
                                Timestamp = timestamp
                            }
                        },
                        ActCommands =
                        {
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamp
                            }
                        },
                        ExpectedEvents =
                        {
                            new AttemptToJoinSuccessfulEvent {
                                ConnectionId = secondPlayer.ConnectionId,
                                JoinedAsPlayerId = secondPlayer.Id,
                                Players = new List<AttemptToJoinSuccessfulEvent.ShortPlayerInfo> {
                                    new AttemptToJoinSuccessfulEvent.ShortPlayerInfo
                                    {
                                        IsConnected = true,
                                        Life = initialLifeAmount,
                                        UserId = firstPlayer.Id,
                                        Name = firstPlayer.Name
                                    },
                                    new AttemptToJoinSuccessfulEvent.ShortPlayerInfo
                                    {
                                        IsConnected = true,
                                        Life = initialLifeAmount,
                                        UserId = secondPlayer.Id,
                                        Name = secondPlayer.Name
                                    }
                                },
                                Timestamp = timestamp,
                            },
                            new PlayerJoinedEvent
                            {
                                LifeAmount = initialLifeAmount,
                                Name = secondPlayer.Name,
                                PlayerId = secondPlayer.Id,
                                Timestamp = timestamp
                            },
                            new GameStartedEvent
                            {
                                Timestamp = timestamp,
                            }
                        }
                    };
                }

                // punch creates damage 
                {
                    var firstPlayer = new
                    {
                        ConnectionId = Guid.NewGuid(),
                        Id = Guid.NewGuid(),
                        Name = "first",
                    };

                    var secondPlayer = new
                    {
                        ConnectionId = Guid.NewGuid(),
                        Id = Guid.NewGuid(),
                        Name = "second",
                    };

                    var timestamp = DateTime.UtcNow;

                    yield return new TestCase
                    {
                        CaseName = "PunchCreatesDamage",
                        Config = roomConfig,
                        RandomValue = 0m,
                        PlayerIds = {
                            firstPlayer.Id,
                            secondPlayer.Id,
                        },
                        PrepareCommands =
                        {
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Name = firstPlayer.Name,
                                Timestamp = timestamp
                            },
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamp
                            }
                        },
                        ActCommands =
                        {
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamp,
                                VictimId = secondPlayer.Id,
                            }
                        },
                        ExpectedEvents =
                        {
                            new PunchEvent
                            {
                                Damage = 5,
                                KillerId = firstPlayer.Id,
                                VictimId = secondPlayer.Id,
                                Timestamp = timestamp
                            }
                        }
                    };
                }
            }
        }

        public class TestCase
        {
            public string CaseName { get; set; }

            public RoomConfig Config { get; set; }

            /// <summary>
            /// See https://xkcd.com/221/
            /// </summary>
            public decimal RandomValue { get; set; } = 0.4m;

            public List<GameCommand> PrepareCommands { get; set; } = new List<GameCommand> { };

            public List<GameCommand> ActCommands { get; set; } = new List<GameCommand> { };

            public List<Guid> PlayerIds { get; set; } = new List<Guid> { };

            public List<GameEvent> ExpectedEvents { get; set; } = new List<GameEvent> { };
        }

        public static IEnumerable<TestCaseData> TestCasesForNUnit
        {
            get
            {
                foreach (var testCase in TestCases)
                {
                    var data = new TestCaseData(testCase);
                    data.SetName("RoomProcessor_" + testCase.CaseName);
                    yield return data;
                }
            }
        }

        public class StubRandomProvider : IRandomProvider
        {
            private readonly decimal chance;

            public StubRandomProvider(decimal chance)
            {
                this.chance = chance;
            }

            public decimal GetNextChance()
            {
                return this.chance;
            }
        }


        public class StubPlayerIdProvider : IPlayerIdGenerator
        {
            private readonly IEnumerator<Guid> playerIds;

            public StubPlayerIdProvider(IEnumerable<Guid> playerIds)
            {
                this.playerIds = playerIds.GetEnumerator();
            }

            public Guid NextPlayerId(RoomState state)
            {
                // TODO: check if we should call it before or after Current
                this.playerIds.MoveNext();
                return this.playerIds.Current;
            }
        }

        // This is not best (slow in runtime), but one of fastest way to implement value comparsion
        public Func<object, object, bool> MakeJsonValueComparer()
        {
            return (a, b) =>
            {
                var serializeSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DefaultValueHandling = DefaultValueHandling.Include,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TypeNameHandling = TypeNameHandling.All
                };

                var serializedA = JsonConvert.SerializeObject(a, serializeSettings);
                var serializedB = JsonConvert.SerializeObject(b, serializeSettings);

                return serializedA == serializedB;
            };
        }
    }
}