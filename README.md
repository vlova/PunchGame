# What it is
 
Test project for gamedev company

# Game idea

Just simply punch each other - use console input for that.
Those who left alive - they are winners

Two console commands:
- punch %username%
- fight more

# Overall

## Overall architecture

Components:
1. Client without predictions
2. Single process server with multiple rooms

Highlights:
- Communication via TCP+JSON
- When joined client will receive room state. Later will receive only events. He should manually update state to get idea where he is.
- In order to change room, client need to reconnect server

## Connection flow

1. Client joins server
2. Servers find free room (or creates it) and associates that room with client
3. Server sends to client number of room
4. Client should send connect to room command with name of user
5. After that client either will see reject or success event
6. In case if reject happens because of stupid reason (room is filled or name is not unique), client will try reconnect automatically

# Server 

## Overall architecture

- TcpGameServer is transport wrapper over GameServer. Later transport can be replaced with something else
- GameServer is responsible for managing clients & rooms & relations of clients/rooms (scheduler)
- GameClient should be transport-independent representation of connection with client (but it's actually coupled with tcp stack)
- RoomServer is responsible for storing room state & handling all commands of clients

## Game (scheduler) architecture

There are no architecture, we try to avoid any logic in scheduler architecture.
Now the logic consists:
- free room search during initial connect
- free room search when room is destroyed

Please, note, that commands are part of game-room, not of game-scheduler
1. Client can't ask for changing room
2. Client sends name not to scheduler, but to specific room

Such tradeoffs leaves us with very simple overall architecture

## Game (room) architecture

Components:
- Mutable state
- Commands & command handlers
- Events & event reducers

Flow:
1. Received tcp event is transformed into one of GameCommands
2. Game commands are grouped by time quant. By this we ensure funny behavior like "two players can kill each other". But this is not required by tech task
3. For each quant we do next:
   1. Clone state so we can know which state was before commands are executed in this quant
   2 For each command
     1. Execute command handler, which is producing events only (doesn't mutate state)
     2. Execute event reducers, which are mutating state only (doesn't produce more events yet)
4. Transmit events:
   - Broadcast events to everyone who plays in room
   - Personal events to specific player
   - Internal events to scheduler subsystem

# Client

Highlights:
- Client references Server.Room.Core and Server.CrossCutting to be able to reproduce same behavior as server

Components
- Client.Core - game logic (non pure)
- Client.Ui - render of ui + console controller
- Client.Network - network layer
- Client.App - stuff that brings everything together


# Code quality

- PunchGame.Client.App
  - It's not really completed
  - No reading of config
- PunchGame.Client.Core
  - This sucks a lot, but who cares
  - GameSession.cs: communication using blocking collections, need find a better solution
  - Error handling sucks
- PunchGame.Client.Network
  - This sucks a lot, but who cares
  - TcpGameSession.cs: communication using blocking collections, need find a better solution
  - Error handling sucks
- PunchGame.Client.Ui
  - This sucks a lot, but who cares
- PunchGame.Server.App
  - This sucks a lot, but who cares
  - No reading of config
  - Should consider move Tcp stuff into Networking assembly
  - TcpGameServer.cs: communication using blocking collections, need find a better solution
  - Error handling sucks
- PunchGame.Server.Core
  - This is almost acceptable for some purposes
  - No logging
  - GameClient.cs: communication using blocking collections, need find a better solution
  - Error handling sucks
- PunchGame.Server.CrossCutting
  - No DI? Lol
- PunchGame.Server.Room.Core
  - Good project & it's important, because it's a business logic
- PunchGame.Server.Server.Tests
  - Only basic tests are present

 Overall: production-unready code