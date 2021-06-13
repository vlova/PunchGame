using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PunchGame.Server.CrossCutting;
using PunchGame.Server.Room.Core.Configs;
using PunchGame.Server.Room.Core.Input;
using PunchGame.Server.Room.Core.Logic;
using PunchGame.Server.Room.Core.Logic.Connection;
using PunchGame.Server.Room.Core.Logic.Game;
using PunchGame.Server.Room.Core.Logic.GeneralGameState;
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
            var randomProvider = new StubRandomProvider(testCase.RandomValue);
            var playerIdGenerator = new StubPlayerIdProvider(testCase.PlayerIds);

            var room = ServerModule.BuildRoomProcessor(testCase.Config, playerIdGenerator, randomProvider);
            var roomState = room.MakeInitialState(testCase.RoomId);

            var _ = room.Process(
              roomState,
              testCase.PrepareCommands);

            var producedEvents = room.Process(
              roomState,
              testCase.ActCommands);

            Assert.That(producedEvents,
                new CollectionEquivalentConstraint(testCase.ExpectedEvents).Using(MakeJsonValueComparer()));
        }

        public static IEnumerable<TestCase> TestCases
        {
            get
            {
                // single player join
                {
                    var playerConnectionId = Guid.NewGuid();
                    var playerName = "trololo";
                    var timestamp = DateTime.UtcNow;
                    var playerId = Guid.NewGuid();

                    yield return new TestCase
                    {
                        CaseName = "SinglePlayerJoin",
                        Config = GetRoomConfig(life: 100),
                        PlayerIds = {
                            playerId,
                        },
                        ActCommands =
                        {
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = playerConnectionId,
                                Name = playerName,
                                Timestamp = timestamp,
                                ClientVersion = 1
                            }
                        },
                        ExpectedEvents =
                        {
                            new AttemptToJoinSuccessfulEvent {
                                ConnectionId = playerConnectionId,
                                JoinedAsPlayerId = playerId,
                                Players = new List<AttemptToJoinSuccessfulEvent.ShortPlayerInfo> {
                                },
                                Timestamp = timestamp,
                            },
                            new PlayerJoinedEvent
                            {
                                LifeAmount = 100,
                                Name = playerName,
                                PlayerId = playerId,
                                Timestamp = timestamp
                            }
                        }
                    };
                }

                // join of second player 
                {
                    var roomId = Guid.NewGuid();

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
                        RoomId = roomId,
                        Config = GetRoomConfig(life: 100),
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
                                Timestamp = timestamp,
                                ClientVersion = 1
                            }
                        },
                        ActCommands =
                        {
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamp,
                                ClientVersion = 1
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
                                        Life = 100,
                                        PlayerId = firstPlayer.Id,
                                        Name = firstPlayer.Name
                                    },
                                },
                                Timestamp = timestamp,
                            },
                            new PlayerJoinedEvent
                            {
                                LifeAmount = 100,
                                Name = secondPlayer.Name,
                                PlayerId = secondPlayer.Id,
                                Timestamp = timestamp
                            },
                            new GameStartedEvent
                            {
                                Timestamp = timestamp,
                            },
                            new RoomFilledEvent
                            {
                                Timestamp = timestamp,
                                RoomId = roomId
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
                        Config = GetRoomConfig(life: 100),
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
                                Timestamp = timestamp,
                                ClientVersion = 1
                            },
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamp,
                                ClientVersion = 1
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

                // second punch doesn't works if it's too near
                {
                    var roomId = Guid.NewGuid();

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

                    var timestamps = new[] {
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddSeconds(2),
                        DateTime.UtcNow.AddSeconds(2)
                    };

                    yield return new TestCase
                    {
                        CaseName = "SecondPunchDoesntWorks",
                        RoomId = roomId,
                        Config = GetRoomConfig(life: 10),
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
                                Timestamp = timestamps[0],
                                ClientVersion = 1
                            },
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamps[0],
                                ClientVersion = 1
                            },
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamps[1],
                                VictimId = secondPlayer.Id,
                            }
                        },
                        ActCommands =
                        {
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamps[2],
                                VictimId = secondPlayer.Id,
                            }
                        },
                        ExpectedEvents =
                        {
                        }
                    };
                }

                // punch creates death & game end 
                {
                    var roomId = Guid.NewGuid();

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

                    var timestamps = new[] {
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddSeconds(2),
                        DateTime.UtcNow.AddSeconds(4)
                    };

                    yield return new TestCase
                    {
                        CaseName = "PunchCreatesDeathAndGameEnd",
                        RoomId = roomId,
                        Config = GetRoomConfig(life: 10),
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
                                Timestamp = timestamps[0],
                                ClientVersion = 1
                            },
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamps[0],
                                ClientVersion = 1
                            },
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamps[1],
                                VictimId = secondPlayer.Id,
                            }
                        },
                        ActCommands =
                        {
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamps[2],
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
                                Timestamp = timestamps[2]
                            },
                            new PlayerDiedEvent
                            {
                                KillerId = firstPlayer.Id,
                                VictimId = secondPlayer.Id,
                                Timestamp = timestamps[2]
                            },
                            new GameEndedEvent
                            {
                                WinnerId = firstPlayer.Id,
                                Timestamp = timestamps[2]
                            },
                            new RoomDestroyedEvent
                            {
                                RoomId = roomId,
                                Timestamp = timestamps[2]
                            }
                        }
                    };
                }

                // punch each other creates death & game end 
                {
                    var roomId = Guid.NewGuid();

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

                    var timestamps = new[] {
                        DateTime.UtcNow,
                        DateTime.UtcNow.AddSeconds(2),
                        DateTime.UtcNow.AddSeconds(4)
                    };

                    yield return new TestCase
                    {
                        CaseName = "PunchEachOtherCreatesDeathAndGameEnd",
                        RoomId = roomId,
                        Config = GetRoomConfig(life: 10),
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
                                Timestamp = timestamps[0],
                                ClientVersion = 1
                            },
                            new ConnectToRoomCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Name = secondPlayer.Name,
                                Timestamp = timestamps[0],
                                ClientVersion = 1
                            },
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamps[1],
                                VictimId = secondPlayer.Id,
                            },
                            new PunchCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Timestamp = timestamps[1],
                                VictimId = firstPlayer.Id,
                            }
                        },
                        ActCommands =
                        {
                            new PunchCommand
                            {
                                ByConnectionId = firstPlayer.ConnectionId,
                                Timestamp = timestamps[2],
                                VictimId = secondPlayer.Id,
                            },
                            new PunchCommand
                            {
                                ByConnectionId = secondPlayer.ConnectionId,
                                Timestamp = timestamps[2],
                                VictimId = firstPlayer.Id,
                            }
                        },
                        ExpectedEvents =
                        {
                            new PunchEvent
                            {
                                Damage = 5,
                                KillerId = firstPlayer.Id,
                                VictimId = secondPlayer.Id,
                                Timestamp = timestamps[2]
                            },
                            new PunchEvent
                            {
                                Damage = 5,
                                KillerId = secondPlayer.Id,
                                VictimId = firstPlayer.Id,
                                Timestamp = timestamps[2]
                            },

                            new PlayerDiedEvent
                            {
                                KillerId = firstPlayer.Id,
                                VictimId = secondPlayer.Id,
                                Timestamp = timestamps[2]
                            },
                            new PlayerDiedEvent
                            {
                                KillerId = secondPlayer.Id,
                                VictimId = firstPlayer.Id,
                                Timestamp = timestamps[2]
                            },
                            
                            // TODO: it would be more nice, if such event will be only one, but with several winnerIds
                            new GameEndedEvent
                            {
                                WinnerId = firstPlayer.Id,
                                Timestamp = timestamps[2]
                            },
                            new GameEndedEvent
                            {
                                WinnerId = secondPlayer.Id,
                                Timestamp = timestamps[2]
                            },

                            // TODO: it would be more nice, if such event will be emitted only once
                            new RoomDestroyedEvent
                            {
                                RoomId = roomId,
                                Timestamp = timestamps[2]
                            },
                            new RoomDestroyedEvent
                            {
                                RoomId = roomId,
                                Timestamp = timestamps[2]
                            }
                        }
                    };
                }
            }
        }

        private static RoomConfig GetRoomConfig(int life)
        {
            return new RoomConfig
            {
                Player = new PlayerConfig
                {
                    InitialLifeAmount = life,
                    Punch = new PunchConfig
                    {
                        Damage = 5,
                        CriticalChance = 0.5m,
                        CriticalDamage = 50,
                        MinimalTimeDiff = TimeSpan.FromSeconds(1)
                    }
                },
                TimeQuant = TimeSpan.FromSeconds(0.1),
                ClientVersion = 1,
                MaxPlayers = 2
            };
        }

        public class TestCase
        {
            public string CaseName { get; set; }

            public RoomConfig Config { get; set; }

            public Guid RoomId { get; set; }

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