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
   2. For each command
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

- No config validation
- Most of parts are not complete
- Communication using blocking collections, need find a better solution
- Several codes run new tasks without exception handling
- There are lot of edge cases not handled properly
- I'm not sure if that really works in likely but not typical cases - didn't had time to test it
- Best part of solution is PunchGame.Server.Core. This is explainable by next: this what needs to be extended on constant basis
- Server: Should consider move Tcp stuff into Networking assembly
- Only basic tests are present

 Overall: production-unready code