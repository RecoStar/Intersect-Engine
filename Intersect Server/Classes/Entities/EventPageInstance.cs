﻿/*
    Intersect Game Engine (Server)
    Copyright (C) 2015  JC Snider, Joe Bridges
    
    Website: http://ascensiongamedev.com
    Contact Email: admin@ascensiongamedev.com 

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System;
using Intersect_Library;
using Intersect_Library.GameObjects;
using Intersect_Library.GameObjects.Events;
using Intersect_Server.Classes.General;
using Intersect_Server.Classes.Misc;
using Intersect_Server.Classes.Misc.Pathfinding;
using Intersect_Server.Classes.Networking;

namespace Intersect_Server.Classes.Entities
{
    public class EventPageInstance : Entity
    {
        public Client Client = null;
        public int Trigger;
        public int MovementType;
        public int MovementFreq;
        public int MovementSpeed;
        public EventGraphic MyGraphic = new EventGraphic();
        public int DisablePreview;
        public EventBase BaseEvent;
        public EventPage MyPage;
        public Entities.EventInstance MyEventIndex;
        public EventPageInstance GlobalClone;
        public EventMoveRoute MoveRoute = null;
        private Pathfinder _pathFinder;
        private int WalkingAnim = 0;
        private int DirectionFix = 0;
        private int RenderLevel = 1;
        private int PageNum = 0;
        public EventPageInstance(EventBase myEvent, EventPage myPage, int myIndex, int mapNum, Entities.EventInstance eventIndex, Client client) : base(myIndex)
        {
            BaseEvent = myEvent;
            MyPage = myPage;
            CurrentMap = mapNum;
            CurrentX = eventIndex.CurrentX;
            CurrentY = eventIndex.CurrentY;
            MyName = myEvent.MyName;
            MovementType = MyPage.MovementType;
            MovementFreq = MyPage.MovementFreq;
            MovementSpeed = MyPage.MovementSpeed;
            DisablePreview = MyPage.DisablePreview;
            Trigger = MyPage.Trigger;
            Passable = MyPage.Passable;
            HideName = MyPage.HideName;
            MyEventIndex = eventIndex;
            MoveRoute = new EventMoveRoute();
            MoveRoute.CopyFrom(MyPage.MoveRoute);
            _pathFinder = new Pathfinder(this);
            SetMovementSpeed(MyPage.MovementSpeed);
            MyGraphic.Type = MyPage.Graphic.Type;
            MyGraphic.Filename = MyPage.Graphic.Filename;
            MyGraphic.X = MyPage.Graphic.X;
            MyGraphic.Y = MyPage.Graphic.Y;
            MyGraphic.Width = MyPage.Graphic.Width;
            MyGraphic.Height = MyPage.Graphic.Height;
            MySprite = MyPage.Graphic.Filename;
            DirectionFix = MyPage.DirectionFix;
            WalkingAnim = MyPage.WalkingAnimation;
            RenderLevel = MyPage.Layer;
            if (MyGraphic.Type == 1)
            {
                switch (MyGraphic.Y)
                {
                    case 0:
                        Dir = 1;
                        break;
                    case 1:
                        Dir = 2;
                        break;
                    case 2:
                        Dir = 3;
                        break;
                    case 3:
                        Dir = 0;
                        break;
                }
            }
            if (myPage.Animation > -1)
            {
                Animations.Add(myPage.Animation);
            }
            Face = MyPage.FaceGraphic;
            PageNum = BaseEvent.MyPages.IndexOf(MyPage);
            Client = client;
            SendToClient();
        }
        public EventPageInstance(EventBase myEvent, EventPage myPage, int myIndex, int mapNum, Entities.EventInstance eventIndex, Client client, EventPageInstance globalClone) : base(myIndex)
        {
            BaseEvent = myEvent;
            GlobalClone = globalClone;
            MyPage = myPage;
            CurrentMap = mapNum;
            CurrentX = globalClone.CurrentX;
            CurrentY = globalClone.CurrentY;
            MyName = myEvent.MyName;
            MovementType = globalClone.MovementType;
            MovementFreq = globalClone.MovementFreq;
            MovementSpeed = globalClone.MovementSpeed;
            DisablePreview = globalClone.DisablePreview;
            Trigger = MyPage.Trigger;
            Passable = globalClone.Passable;
            HideName = globalClone.HideName;
            CurrentMap = mapNum;
            MyEventIndex = eventIndex;
            MoveRoute = globalClone.MoveRoute;
            _pathFinder = new Pathfinder(this);
            SetMovementSpeed(MyPage.MovementSpeed);
            MyGraphic.Type = globalClone.MyGraphic.Type;
            MyGraphic.Filename = globalClone.MyGraphic.Filename;
            MyGraphic.X = globalClone.MyGraphic.X;
            MyGraphic.Y = globalClone.MyGraphic.Y;
            MyGraphic.Width = globalClone.MyGraphic.Width;
            MyGraphic.Height = globalClone.MyGraphic.Height;
            MySprite = MyPage.Graphic.Filename;
            DirectionFix = MyPage.DirectionFix;
            WalkingAnim = MyPage.WalkingAnimation;
            RenderLevel = MyPage.Layer;
            if (globalClone.MyGraphic.Type == 1)
            {
                switch (MyPage.Graphic.Y)
                {
                    case 0:
                        Dir = 1;
                        break;
                    case 1:
                        Dir = 2;
                        break;
                    case 2:
                        Dir = 3;
                        break;
                    case 3:
                        Dir = 0;
                        break;
                }
            }
            if (myPage.Animation > -1)
            {
                Animations.Add(myPage.Animation);
            }
            Face = MyPage.FaceGraphic;
            PageNum = BaseEvent.MyPages.IndexOf(MyPage);
            Client = client;
            SendToClient();
        }

        public void SendToClient()
        {
            if (Client != null)
            {
                PacketSender.SendEntityDataTo(Client, MyIndex, (int)EntityTypes.Event, Data(), this);
            }
        }
        public byte[] Data()
        {
            var bf = new ByteBuffer();
            bf.WriteBytes(base.Data());
            bf.WriteInteger(HideName);
            bf.WriteInteger(DirectionFix);
            bf.WriteInteger(WalkingAnim);
            bf.WriteInteger(DisablePreview);
            bf.WriteString(MyPage.Desc);
            bf.WriteInteger(MyGraphic.Type);
            bf.WriteString(MyGraphic.Filename);
            bf.WriteInteger(MyGraphic.X);
            bf.WriteInteger(MyGraphic.Y);
            bf.WriteInteger(MyGraphic.Width);
            bf.WriteInteger(MyGraphic.Height);
            bf.WriteInteger(RenderLevel);
            return bf.ToArray();
        }

        public void SetMovementSpeed(int speed)
        {
            switch (speed)
            {
                case 0:
                    Stat[2].Stat = 5;
                    break;
                case 1:
                    Stat[2].Stat = 10;
                    break;
                case 2:
                    Stat[2].Stat = 20;
                    break;
                case 3:
                    Stat[2].Stat = 30;
                    break;
                case 4:
                    Stat[2].Stat = 40;
                    break;

            }
        }
        public void Update()
        {
            if (MoveTimer >= Environment.TickCount || GlobalClone != null) return;
            if (MovementType == 2 && MoveRoute != null)
            {
                ProcessMoveRoute();
            }
            else
            {
                if (MovementType == 1) //Random
                {
                    if (Globals.Rand.Next(0, 2) != 0) return;
                    var dir = Globals.Rand.Next(0, 4);
                    if (CanMove(dir) == -1)
                    {
                        Move(dir, Client);
                        switch (MovementFreq)
                        {
                            case 0:
                                MoveTimer = Environment.TickCount + 4000;
                                break;
                            case 1:
                                MoveTimer = Environment.TickCount + 2000;
                                break;
                            case 2:
                                MoveTimer = Environment.TickCount + 1000;
                                break;
                            case 3:
                                MoveTimer = Environment.TickCount + 500;
                                break;
                            case 4:
                                MoveTimer = Environment.TickCount + 250;
                                break;
                        }
                    }
                }
            }
        }

        private void ProcessMoveRoute()
        {
            var moved = false;
            var shouldSendUpdate = false;
            int lookDir = 0, moveDir = 0;
            if (MoveRoute.ActionIndex < MoveRoute.Actions.Count)
            {
                switch (MoveRoute.Actions[MoveRoute.ActionIndex].Type)
                {
                    case MoveRouteEnum.MoveUp:
                        if (CanMove((int)Directions.Up) == -1)
                        {
                            Move((int)Directions.Up, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.MoveDown:
                        if (CanMove((int)Directions.Down) == -1)
                        {
                            Move((int)Directions.Down, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.MoveLeft:
                        if (CanMove((int)Directions.Left) == -1)
                        {
                            Move((int)Directions.Left, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.MoveRight:
                        if (CanMove((int)Directions.Right) == -1)
                        {
                            Move((int)Directions.Right, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.MoveRandomly:
                        var dir = Globals.Rand.Next(0, 4);
                        if (CanMove(dir) == -1)
                        {
                            Move(dir, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.MoveTowardsPlayer:
                        //Pathfinding required.. this will be weird.
                        if (Client != null && GlobalClone == null) //Local Event
                        {
                            if (_pathFinder.GetTarget() == null || _pathFinder.GetTarget().TargetMap != Client.Entity.CurrentMap || _pathFinder.GetTarget().TargetX != Client.Entity.CurrentX || _pathFinder.GetTarget().TargetY != Client.Entity.CurrentY)
                            {
                                _pathFinder.SetTarget(new PathfinderTarget(Client.Entity.CurrentMap,
                                    Client.Entity.CurrentX, Client.Entity.CurrentY));
                            }
                            else
                            {
                                if (_pathFinder.GetMove() > -1)
                                {
                                    if (CanMove(_pathFinder.GetMove()) == -1)
                                    {
                                        Move(_pathFinder.GetMove(), Client);
                                        _pathFinder.RemoveMove();
                                        moved = true;
                                    }
                                }
                            }
                        }
                        break;
                    case MoveRouteEnum.MoveAwayFromPlayer:
                        //This won't be anything special.
                        if (Client != null && GlobalClone == null) //Local Event
                        {
                            moveDir = GetDirectionTo(Client.Entity);
                            if (moveDir > -1)
                            {
                                switch (moveDir)
                                {
                                    case (int)Directions.Up:
                                        moveDir = (int)Directions.Down;
                                        break;
                                    case (int)Directions.Down:
                                        moveDir = (int)Directions.Up;
                                        break;
                                    case (int)Directions.Left:
                                        moveDir = (int)Directions.Right;
                                        break;
                                    case (int)Directions.Right:
                                        moveDir = (int)Directions.Left;
                                        break;
                                }
                                if (CanMove(moveDir) == -1)
                                {
                                    Move(moveDir, Client);
                                    moved = true;
                                }
                                else
                                {
                                    //Move Randomly
                                    moveDir = Globals.Rand.Next(0, 4);
                                    if (CanMove(moveDir) == -1)
                                    {
                                        Move(moveDir, Client);
                                        moved = true;
                                    }
                                }
                            }
                            else
                            {
                                //Move Randomly
                                moveDir = Globals.Rand.Next(0, 4);
                                if (CanMove(moveDir) == -1)
                                {
                                    Move(moveDir, Client);
                                    moved = true;
                                }
                            }
                        }
                        break;
                    case MoveRouteEnum.StepForward:
                        if (CanMove(Dir) > -1)
                        {
                            Move(Dir, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.StepBack:
                        switch (Dir)
                        {
                            case (int)Directions.Up:
                                moveDir = (int)Directions.Down;
                                break;
                            case (int)Directions.Down:
                                moveDir = (int)Directions.Up;
                                break;
                            case (int)Directions.Left:
                                moveDir = (int)Directions.Right;
                                break;
                            case (int)Directions.Right:
                                moveDir = (int)Directions.Left;
                                break;
                        }
                        if (CanMove(moveDir) > -1)
                        {
                            Move(moveDir, Client);
                            moved = true;
                        }
                        break;
                    case MoveRouteEnum.FaceUp:
                        ChangeDir((int)Directions.Up);
                        moved = true;
                        break;
                    case MoveRouteEnum.FaceDown:
                        ChangeDir((int)Directions.Down);
                        moved = true;
                        break;
                    case MoveRouteEnum.FaceLeft:
                        ChangeDir((int)Directions.Left);
                        moved = true;
                        break;
                    case MoveRouteEnum.FaceRight:
                        ChangeDir((int)Directions.Right);
                        moved = true;
                        break;
                    case MoveRouteEnum.Turn90Clockwise:
                        switch (Dir)
                        {
                            case (int)Directions.Up:
                                lookDir = (int)Directions.Right;
                                break;
                            case (int)Directions.Down:
                                lookDir = (int)Directions.Left;
                                break;
                            case (int)Directions.Left:
                                lookDir = (int)Directions.Down;
                                break;
                            case (int)Directions.Right:
                                lookDir = (int)Directions.Up;
                                break;
                        }
                        ChangeDir(lookDir);
                        moved = true;
                        break;
                    case MoveRouteEnum.Turn90CounterClockwise:
                        switch (Dir)
                        {
                            case (int)Directions.Up:
                                lookDir = (int)Directions.Left;
                                break;
                            case (int)Directions.Down:
                                lookDir = (int)Directions.Right;
                                break;
                            case (int)Directions.Left:
                                lookDir = (int)Directions.Up;
                                break;
                            case (int)Directions.Right:
                                lookDir = (int)Directions.Down;
                                break;
                        }
                        ChangeDir(lookDir);
                        moved = true;
                        break;
                    case MoveRouteEnum.Turn180:
                        switch (Dir)
                        {
                            case (int)Directions.Up:
                                lookDir = (int)Directions.Down;
                                break;
                            case (int)Directions.Down:
                                lookDir = (int)Directions.Up;
                                break;
                            case (int)Directions.Left:
                                lookDir = (int)Directions.Right;
                                break;
                            case (int)Directions.Right:
                                lookDir = (int)Directions.Left;
                                break;
                        }
                        ChangeDir(lookDir);
                        moved = true;
                        break;
                    case MoveRouteEnum.TurnRandomly:
                        ChangeDir(Globals.Rand.Next(0, 4));
                        moved = true;
                        break;
                    case MoveRouteEnum.FacePlayer:
                        if (Client != null && GlobalClone == null) //Local Event
                        {
                            lookDir = GetDirectionTo(Client.Entity);
                            if (lookDir > -1)
                            {
                                ChangeDir(lookDir);
                                moved = true;
                            }
                        }
                        break;
                    case MoveRouteEnum.FaceAwayFromPlayer:
                        if (Client != null && GlobalClone == null) //Local Event
                        {
                            lookDir = GetDirectionTo(Client.Entity);
                            if (lookDir > -1)
                            {
                                switch (lookDir)
                                {
                                    case (int)Directions.Up:
                                        lookDir = (int)Directions.Down;
                                        break;
                                    case (int)Directions.Down:
                                        lookDir = (int)Directions.Up;
                                        break;
                                    case (int)Directions.Left:
                                        lookDir = (int)Directions.Right;
                                        break;
                                    case (int)Directions.Right:
                                        lookDir = (int)Directions.Left;
                                        break;
                                }
                                ChangeDir(lookDir);
                                moved = true;
                            }
                        }
                        break;
                    case MoveRouteEnum.SetSpeedSlowest:
                        SetMovementSpeed(0);
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetSpeedSlower:
                        SetMovementSpeed(1);
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetSpeedNormal:
                        SetMovementSpeed(2);
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetSpeedFaster:
                        SetMovementSpeed(3);
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetSpeedFastest:
                        SetMovementSpeed(4);
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetFreqLowest:
                        MovementFreq = 0;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetFreqLower:
                        MovementFreq = 1;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetFreqNormal:
                        MovementFreq = 2;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetFreqHigher:
                        MovementFreq = 3;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetFreqHighest:
                        MovementFreq = 4;
                        moved = true;
                        break;
                    case MoveRouteEnum.WalkingAnimOn:
                        WalkingAnim = 1;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.WalkingAnimOff:
                        WalkingAnim = 0;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.DirectionFixOn:
                        DirectionFix = 1;
                        moved = true;
                        break;
                    case MoveRouteEnum.DirectionFixOff:
                        DirectionFix = 0;
                        moved = true;
                        break;
                    case MoveRouteEnum.WalkthroughOn:
                        Passable = 1;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.WalkthroughOff:
                        Passable = 0;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.ShowName:
                        HideName = 0;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.HideName:
                        HideName = 1;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetLevelBelow:
                        RenderLevel = 0;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetLevelNormal:
                        RenderLevel = 1;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetLevelAbove:
                        RenderLevel = 2;
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.Wait100:
                        MoveTimer = Environment.TickCount + 100;
                        moved = true;
                        break;
                    case MoveRouteEnum.Wait500:
                        MoveTimer = Environment.TickCount + 500;
                        moved = true;
                        break;
                    case MoveRouteEnum.Wait1000:
                        MoveTimer = Environment.TickCount + 1000;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetGraphic:
                        MyGraphic.Type = MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.Type;
                        MyGraphic.Filename = MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.Filename;
                        MyGraphic.X = MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.X;
                        MyGraphic.Y = MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.Y;
                        MyGraphic.Width = MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.Width;
                        MyGraphic.Height = MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.Height;
                        if (MyGraphic.Type == 1)
                        {
                            switch (MoveRoute.Actions[MoveRoute.ActionIndex].Graphic.Y)
                            {
                                case 0:
                                    Dir = 1;
                                    break;
                                case 1:
                                    Dir = 2;
                                    break;
                                case 2:
                                    Dir = 3;
                                    break;
                                case 3:
                                    Dir = 0;
                                    break;
                            }
                        }
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    case MoveRouteEnum.SetAnimation:
                        Animations.Clear();
                        var anim = AnimationBase.GetAnim(MoveRoute.Actions[MoveRoute.ActionIndex].AnimationIndex);
                        if (anim != null)
                        {
                            Animations.Add(MoveRoute.Actions[MoveRoute.ActionIndex].AnimationIndex);
                        }
                        shouldSendUpdate = true;
                        moved = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (moved || MoveRoute.IgnoreIfBlocked)
                {
                    MoveRoute.ActionIndex++;
                    if (MoveRoute.ActionIndex >= MoveRoute.Actions.Count)
                    {
                        if (MoveRoute.RepeatRoute) MoveRoute.ActionIndex = 0;
                        MoveRoute.Complete = true;
                    }
                }
                if (shouldSendUpdate)
                {
                    //Send Update
                    SendToClient();
                }
                if (MoveTimer < Environment.TickCount)
                {
                    switch (MovementFreq)
                    {
                        case 0:
                            MoveTimer = Environment.TickCount + 4000;
                            break;
                        case 1:
                            MoveTimer = Environment.TickCount + 2000;
                            break;
                        case 2:
                            MoveTimer = Environment.TickCount + 1000;
                            break;
                        case 3:
                            MoveTimer = Environment.TickCount + 500;
                            break;
                        case 4:
                            MoveTimer = Environment.TickCount + 250;
                            break;
                    }
                }
            }
        }

        public override int CanMove(int moveDir)
        {
            switch (moveDir)
            {
                case (int)Directions.Up:
                    if (CurrentY == 0) return -5;
                    break;
                case (int)Directions.Down:
                    if (CurrentY == Options.MapHeight - 1) return -5;
                    break;
                case (int)Directions.Left:
                    if (CurrentX == 0) return -5;
                    break;
                case (int)Directions.Right:
                    if (CurrentX == Options.MapWidth - 1) return -5;
                    break;
            }
            return base.CanMove(moveDir);
        }

        public void TurnTowardsPlayer()
        {
            int lookDir = -1;
            if (Client != null && GlobalClone == null) //Local Event
            {
                lookDir = GetDirectionTo(Client.Entity);
                if (lookDir > -1)
                {
                    ChangeDir(lookDir);
                }
            }
        }

        public bool ShouldDespawn()
        {
            //Should despawn if conditions are not met OR an earlier page can page
            for (int i = 0; i < MyPage.Conditions.Count; i++)
            {
                if (!MyEventIndex.MeetsConditions(MyPage.Conditions[i]))
                {
                    return true;
                }
            }
            for (int i = 0; i < BaseEvent.MyPages.Count; i++)
            {
                if (MyEventIndex.CanSpawnPage(i, BaseEvent))
                {
                    if (i > PageNum) return true;
                }
            }
            return false;
        }
    }
}
