using Fisobs.Creatures;
using Noise;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;
using Fisobs.Core;
using Fisobs.Properties;
using Fisobs.Sandbox;
using CreatureType = CreatureTemplate.Type;
using static PathCost.Legality;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;
using System.Runtime.CompilerServices;

namespace thelost
{
    internal class Squabcritob : Critob
    {
        public static readonly CreatureType Squab = new("Squab", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID SquabUnlock = new("Squab", true);
        public Squabcritob() : base(Squab)
        {
            LoadedPerformanceCost = 20f;
            SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
            ShelterDanger = ShelterDanger.Safe;
            CreatureName = "Squab";

            RegisterUnlock(killScore: KillScore.Configurable(1), SquabUnlock, parent: MultiplayerUnlocks.SandboxUnlockID.LanternMouse, data: 0);
        }

        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new SquabAI(acrit, acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new Squab(acrit, acrit.world);
        }
        public override CreatureState CreateState(AbstractCreature acrit)
        {
            return new SquabState(acrit);
        }
        public override IEnumerable<string> WorldFileAliases()
        {
            return new[] { "squab", "Squab" };
        }

        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate t = new CreatureFormula(this)
            {
                DefaultRelationship = new(CreatureTemplate.Relationship.Type.Afraid, 0.5f),
                HasAI = true,
                InstantDeathDamage = 1,
                Pathing = PreBakedPathing.Ancestral(CreatureType.LanternMouse),
                TileResistances = new()
                {
                    Floor = new(1,Allowed),
                    Climb = new(1,Allowed),
                    OffScreen = new(1,Allowed),
                    Corridor = new(1,Allowed),
                },
                ConnectionResistances = new()
                {
                    Standard = new(1,Allowed),
                    OpenDiagonal = new(1,Allowed),
                    ShortCut = new(1,Allowed),
                    NPCTransportation = new(1,Allowed),
                    OffScreenMovement = new(1,Allowed),
                    BetweenRooms = new(1,Allowed),
                    DropToFloor = new(1,Allowed),
                    Slope = new(1,Allowed),
                },
                DamageResistances = new()
                {
                    Base=0.95f,
                },
                StunResistances = new()
                {
                    Base = 0.6f,
                },
                
            }.IntoTemplate();
            t.offScreenSpeed = 0.4f;
            t.abstractedLaziness = 100;
            t.roamBetweenRoomsChance = 0.5f;
            t.bodySize = 0.3f;
            t.stowFoodInDen = false;
            t.shortcutSegments = 1;
            t.grasps = 1;
            t.visualRadius = 800f;
            t.deliciousness = 0.9f;
            t.waterRelationship = CreatureTemplate.WaterRelationship.AirOnly;
            t.waterPathingResistance = 1f;
            t.meatPoints = 2;
            t.dangerousToPlayer = 0f;
            t.quickDeath = true;
            return t;
        }

        public override void EstablishRelationships()
        {
            Relationships self = new(Squab);
            foreach(var template in StaticWorld.creatureTemplates)
            {
                if (template.quantified)
                {
                    self.Ignores(template.type);
                    self.IgnoredBy(template.type);
                }
            }
            self.Fears(CreatureType.LizardTemplate, 1f);
            self.EatenBy(CreatureType.LizardTemplate, 1f);
            self.Fears(CreatureType.Scavenger, 1f);
            self.Fears(CreatureType.BigSpider, 1f);
            self.EatenBy(CreatureType.BigSpider, 1f);
            self.HasDynamicRelationship(CreatureType.Slugcat, 1f);
        }
    }
    internal class SquabAI : ArtificialIntelligence, FriendTracker.IHaveFriendTracker, IUseARelationshipTracker
    {
        public Squab squab;

        public class Behavior : ExtEnum<SquabAI.Behavior>
        {
            public Behavior(string value, bool register = false) : base(value, register) { }

            public static readonly SquabAI.Behavior Idle = new SquabAI.Behavior("Idle", true);

            public static readonly SquabAI.Behavior Flee = new SquabAI.Behavior("Flee", true);

            public static readonly SquabAI.Behavior Hunt = new SquabAI.Behavior("Hunt", true);

            public static readonly SquabAI.Behavior EscapeRain = new SquabAI.Behavior("EscapeRain", true);

            public static readonly SquabAI.Behavior ReturnPrey = new SquabAI.Behavior("ReturnPrey", true);

            public static readonly SquabAI.Behavior Snack = new SquabAI.Behavior("Snack", true);

            public static readonly SquabAI.Behavior InvestigateSound = new SquabAI.Behavior("InvestigateSound", true);

            public static readonly SquabAI.Behavior FollowFriend = new("FollowFriend", true);
        }
        public SquabAI.Behavior behavior;
        public SquabAI(AbstractCreature creature, World world) : base(creature, world)
        {
            this.squab = (creature.realizedCreature as Squab);
            this.squab.ai = this;
            base.AddModule(new StandardPather(this, world, creature));
            base.AddModule(new Tracker(this, 10, 10, 450, 0.5f, 5, 5, 10));
            base.AddModule(new ThreatTracker(this, 3));
            base.AddModule(new RainTracker(this));
            base.AddModule(new DenFinder(this, creature));
            base.AddModule(new UtilityComparer(this));
            base.AddModule(new RelationshipTracker(this, base.tracker));
            base.AddModule(new FriendTracker(this));
            base.AddModule(new NoiseTracker(this, this.tracker));
            base.utilityComparer.AddComparedModule(base.threatTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.rainTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.friendTracker, null, 1f, 1.1f);
            base.utilityComparer.AddComparedModule(base.noiseTracker, null, 0.6f, 1.1f);
            this.behavior = Behavior.Idle;

            
        }
        public void GiftRecieved(SocialEventRecognizer.OwnedItemOnGround gift)
        {
            throw new NotImplementedException();
        }
       
        public AIModule ModuleToTrackRelationship(CreatureTemplate.Relationship relationship)
        {
            if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
            {
                return base.threatTracker;
            }
            return null;
        }

        public override PathCost TravelPreference(MovementConnection coord, PathCost cost)
        {
            if (this.behavior != SquabAI.Behavior.Flee)
            {
                return cost;
            }
            return new PathCost(cost.resistance + base.threatTracker.ThreatOfTile(coord.destinationCoord, true) * 100f, cost.legality);
        }
        public float currentUtility;
        public override void Update()
        {
            base.Update();
            if(this.squab.room == null) { return; }
            if (ModManager.MSC && this.squab.LickedByPlayer != null)
            {
                base.tracker.SeeCreature(this.squab.LickedByPlayer.abstractCreature);
            }
            base.pathFinder.walkPastPointOfNoReturn = (this.stranded || base.denFinder.GetDenPosition() == null || !base.pathFinder.CoordinatePossibleToGetBackFrom(base.denFinder.GetDenPosition().Value) || base.threatTracker.Utility() > 0.95f);
            AIModule aimodule = base.utilityComparer.HighestUtilityModule();
            this.currentUtility = base.utilityComparer.HighestUtility();
            if(aimodule != null)
            {
                if(aimodule is ThreatTracker)
                {
                    this.behavior = Behavior.Flee;
                } else if(aimodule is RainTracker) {
                    this.behavior = Behavior.EscapeRain;

                }else if(aimodule is FriendTracker)
                {
                    this.behavior = Behavior.FollowFriend;
                } else if(aimodule is NoiseTracker) {
                    this.behavior = Behavior.InvestigateSound;
                }
            }
            if (this.currentUtility < 0.2f)
            {
                this.behavior = SquabAI.Behavior.Idle;
            }
            if (this.behavior == Behavior.Idle)
            {
                List<WorldCoordinate> fruit = new List<WorldCoordinate>();
                for(int j = 0;j< this.squab.room.physicalObjects.Length; j++)
                {
                    PhysicalObject items = (PhysicalObject)this.squab.room.physicalObjects.GetValue(j);
                    if (items is DangleFruit)
                    {
                        for (int i = items.abstractPhysicalObject.pos.y; i > 0; i--)
                        {
                            if(this.squab.room.GetTile(new Vector2(items.abstractPhysicalObject.pos.x, i)).Solid)
                            {
                                if (base.pathFinder.CoordinateReachableAndGetbackable(this.squab.room.GetWorldCoordinate(new IntVector2(items.abstractPhysicalObject.pos.x, i)))){
                                    fruit.Add(this.squab.room.GetWorldCoordinate(new IntVector2(items.abstractPhysicalObject.pos.x, i)));
                                }
                            }
                        }
                    }
                }
                if(fruit.Count > 0)
                {
                    int chosenfruit = Random.Range(0,fruit.Count);
                    this.creature.abstractAI.SetDestination(fruit[chosenfruit]);
                }
                if (!base.pathFinder.CoordinateReachableAndGetbackable(base.pathFinder.GetDestination))
                {
                    this.creature.abstractAI.SetDestination(this.creature.pos);
                }
                bool flag = base.pathFinder.GetDestination.room != this.squab.room.abstractRoom.index;
                if (!flag && this.idlePosCounter > 1200)
                {
                    int abstractNode = this.squab.room.abstractRoom.RandomNodeInRoom().abstractNode;
                    if (this.squab.room.abstractRoom.nodes[abstractNode].type == AbstractRoomNode.Type.Exit)
                    {
                        int num = this.squab.room.abstractRoom.CommonToCreatureSpecificNodeIndex(abstractNode, this.squab.Template);
                        if (num > -1 && this.squab.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(this.squab.abstractCreature.pos.Tile, num, this.squab.Template) > -1)
                        {
                            AbstractRoom abstractRoom = this.squab.room.game.world.GetAbstractRoom(this.squab.room.abstractRoom.connections[abstractNode]);
                            if (abstractRoom != null)
                            {
                                WorldCoordinate worldCoordinate = abstractRoom.RandomNodeInRoom();
                                if (base.pathFinder.CoordinateReachableAndGetbackable(worldCoordinate))
                                {
                                    this.creature.abstractAI.SetDestination(worldCoordinate);
                                    this.idlePosCounter = Random.Range(200, 500);
                                    flag = true;
                                }
                            }
                        }
                    }
                }
                if (!flag && (Random.value < 0.0045454544f || this.idlePosCounter <= 0))
                {
                    IntVector2 pos = new IntVector2(Random.Range(0, this.squab.room.TileWidth), Random.Range(0, this.squab.room.TileHeight));
                    if (base.pathFinder.CoordinateReachableAndGetbackable(this.squab.room.GetWorldCoordinate(pos)))
                    {
                        this.creature.abstractAI.SetDestination(this.squab.room.GetWorldCoordinate(pos));
                        this.idlePosCounter = Random.Range(200, 1900);
                    }
                }
                idlePosCounter--;
            }
            else if(this.behavior == Behavior.Flee)
            {
                this.creature.abstractAI.SetDestination(base.threatTracker.FleeTo(this.creature.pos, 6, 20, true));
                
            }else if(this.behavior == Behavior.EscapeRain)
            {
                if(base.denFinder.GetDenPosition() != null)
                {
                    this.creature.abstractAI.SetDestination(base.denFinder.GetDenPosition().Value);
                }
            } else if (this.behavior == Behavior.InvestigateSound)
            {
                this.creature.abstractAI.SetDestination(base.noiseTracker.ExaminePos);
            } else if(this.behavior == Behavior.FollowFriend)
            {
                this.creature.abstractAI.SetDestination(base.friendTracker.friendDest);
                if(this.creature.pos == base.friendTracker.friendDest && this.squab.grasps[0].grabbed is DangleFruit)
                {
                    this.squab.ReleaseGrasp(0);
                }
               
            }

        }
        public int idlePosCounter = 0;
        public CreatureTemplate.Relationship UpdateDynamicRelationship(RelationshipTracker.DynamicRelationship dRelation)
        {
            if (dRelation.trackerRep.visualContact)
            {
                dRelation.state.alive = dRelation.trackerRep.representedCreature.state.alive;
            }
            CreatureTemplate.Relationship relationship = base.StaticRelationship(dRelation.trackerRep.representedCreature);
            if (relationship.type == CreatureTemplate.Relationship.Type.Afraid)
            {
                if (!dRelation.state.alive)
                {
                    relationship.intensity = 0f;
                } else if(dRelation.trackerRep.BestGuessForPosition().room == this.squab.room.abstractRoom.index && !dRelation.trackerRep.representedCreature.creatureTemplate.canFly)
                {
                    float num = Mathf.Lerp(0.1f, 1.6f, Mathf.InverseLerp(-100f, 200f, this.squab.room.MiddleOfTile(dRelation.trackerRep.BestGuessForPosition().Tile).y - this.squab.mainBodyChunk.pos.y));
                    float value = float.MaxValue;
                    num = Mathf.Lerp(num, 1f, Mathf.InverseLerp(50f, 500f, value));
                    relationship.intensity *= num;
                }
            }
            return relationship;
        }

        public override bool WantToStayInDenUntilEndOfCycle()
        {
            return base.rainTracker.Utility() > 0.01f;
        }

        public RelationshipTracker.TrackedCreatureState CreateTrackedCreatureState(RelationshipTracker.DynamicRelationship rel)
        {
            return null;
        }
    }

    internal class SquabState : HealthState
    {
        public SquabState(AbstractCreature creature) : base(creature)
        {
            //(this.creature.realizedCreature as Squab).State = this;
        }
    }
    internal class Squab : AirBreatherCreature
    {
        //this is to make rebinding easier later:
        public KeyCode bringMyLittleBoy = KeyCode.B;
        public Squab(AbstractCreature abstrCrit, World world) : base(abstrCrit, world)
        {
            base.bodyChunks = new BodyChunk[2];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 4f, 0.2f);
            base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 2f, 0.2f);
            this.bodyChunkConnections = new PhysicalObject.BodyChunkConnection[1];
            this.bodyChunkConnections[0] = new PhysicalObject.BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 6f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
            base.airFriction = 0.9f;
            base.gravity = 0.9f;
            this.bounce = 0.5f;
            this.surfaceFriction = 0.6f;
            this.collisionLayer = 1;
            base.waterFriction = 0.7f;
            base.buoyancy = 0.95f;
            
        }
        public void Act()
        {
            MovementConnection movementConnection = (this.ai.pathFinder as StandardPather).FollowPath(this.room.GetWorldCoordinate(base.mainBodyChunk.pos), true);
            if (movementConnection == null)
            {
                movementConnection = (this.ai.pathFinder as StandardPather).FollowPath(this.room.GetWorldCoordinate(base.bodyChunks[1].pos), true);
            }
            if (base.abstractCreature.controlled && (movementConnection == null || !this.AllowableControlledAIOverride(movementConnection.type)))
            {
                movementConnection = null;
                if (this.inputWithDiagonals != null)
                {
                    MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
                    if (movementConnection != null)
                    {
                        type = movementConnection.type;
                    }
                    if (this.room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
                    {
                        type = MovementConnection.MovementType.ShortCut;
                    }
                }
            }
            if (movementConnection != null)
            {
                this.Run(movementConnection);
            }
            else
            {
                base.GoThroughFloors = false;
            }
        }
        private void Swim()
        {
            BodyChunk mainBodyChunk = base.mainBodyChunk;
            mainBodyChunk.vel.y = mainBodyChunk.vel.y + 1.5f;
        }
        MovementConnection lastFollowedConnection;
        bool currentlyClimbingCorridor;
        int footingCounter;
        public bool Footing
        {
            get
            {
                return this.footingCounter > 20;
            }
        }
        int specialMoveCounter;
        IntVector2 specialMoveDestination;
        float runSpeed = 2f;
        private void MoveTowards(Vector2 moveTo)
        {
            Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, moveTo);
            if (this.room.aimap.getAItile(base.bodyChunks[1].pos).acc >= AItile.Accessibility.Climb)
            {
                vector *= 0.5f;
            }
            if (!this.Footing)
            {
                vector *= 0.3f;
            }
            if (base.IsTileSolid(1, 0, -1) && (((double)vector.x < -0.5 && base.mainBodyChunk.pos.x > base.bodyChunks[1].pos.x + 5f) || ((double)vector.x > 0.5 && base.mainBodyChunk.pos.x < base.bodyChunks[1].pos.x - 5f)))
            {
                BodyChunk mainBodyChunk = base.mainBodyChunk;
                mainBodyChunk.vel.x = mainBodyChunk.vel.x - ((vector.x < 0f) ? -1f : 1f) * 1.3f;
                BodyChunk bodyChunk = base.bodyChunks[1];
                bodyChunk.vel.x = bodyChunk.vel.x + ((vector.x < 0f) ? -1f : 1f) * 0.5f;
                if (!base.IsTileSolid(0, 0, 1))
                {
                    BodyChunk mainBodyChunk2 = base.mainBodyChunk;
                    mainBodyChunk2.vel.y = mainBodyChunk2.vel.y + 3.2f;
                }
            }
            base.mainBodyChunk.vel += vector * 4.2f * this.runSpeed;
            base.bodyChunks[1].vel -= vector * 1f * this.runSpeed;
            base.GoThroughFloors = (moveTo.y < base.mainBodyChunk.pos.y - 5f);
        }
        private void Run(MovementConnection followingConnection)
        {
            if (followingConnection.destinationCoord.y > followingConnection.startCoord.y && this.room.aimap.getAItile(followingConnection.destinationCoord).acc != AItile.Accessibility.Climb)
            {
                this.currentlyClimbingCorridor = true;
            }
            if (followingConnection.type == MovementConnection.MovementType.ReachUp)
            {
                (this.ai.pathFinder as StandardPather).pastConnections.Clear();
            }
            if (followingConnection.type == MovementConnection.MovementType.ShortCut || followingConnection.type == MovementConnection.MovementType.NPCTransportation)
            {
                this.enteringShortCut = new IntVector2?(followingConnection.StartTile);
                if (base.abstractCreature.controlled)
                {
                    bool flag = false;
                    List<IntVector2> list = new List<IntVector2>();
                    foreach (ShortcutData shortcutData in this.room.shortcuts)
                    {
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != followingConnection.StartTile)
                        {
                            list.Add(shortcutData.StartTile);
                        }
                        if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == followingConnection.StartTile)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        if (list.Count > 0)
                        {
                            list.Shuffle<IntVector2>();
                            this.NPCTransportationDestination = this.room.GetWorldCoordinate(list[0]);
                        }
                        else
                        {
                            this.NPCTransportationDestination = followingConnection.destinationCoord;
                        }
                    }
                }
                else if (followingConnection.type == MovementConnection.MovementType.NPCTransportation)
                {
                    this.NPCTransportationDestination = followingConnection.destinationCoord;
                }
            }
            else if (followingConnection.type == MovementConnection.MovementType.OpenDiagonal || followingConnection.type == MovementConnection.MovementType.ReachOverGap || followingConnection.type == MovementConnection.MovementType.ReachUp || followingConnection.type == MovementConnection.MovementType.ReachDown || followingConnection.type == MovementConnection.MovementType.SemiDiagonalReach)
            {
                this.specialMoveCounter = 30;
                this.specialMoveDestination = followingConnection.DestTile;
            }
            else
            {
                Vector2 vector = this.room.MiddleOfTile(followingConnection.DestTile);
                if (this.lastFollowedConnection != null && this.lastFollowedConnection.type == MovementConnection.MovementType.ReachUp)
                {
                    base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, vector) * 4f;
                }
                if (this.Footing)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (followingConnection.startCoord.x == followingConnection.destinationCoord.x)
                        {
                            BodyChunk bodyChunk = base.bodyChunks[j];
                            bodyChunk.vel.x = bodyChunk.vel.x + Mathf.Min((vector.x - base.bodyChunks[j].pos.x) / 8f, 1.2f);
                        }
                        else if (followingConnection.startCoord.y == followingConnection.destinationCoord.y)
                        {
                            BodyChunk bodyChunk2 = base.bodyChunks[j];
                            bodyChunk2.vel.y = bodyChunk2.vel.y + Mathf.Min((vector.y - base.bodyChunks[j].pos.y) / 8f, 1.2f);
                        }
                    }
                }
                if (this.lastFollowedConnection != null && (this.Footing || this.room.aimap.TileAccessibleToCreature(base.mainBodyChunk.pos, base.Template)) && ((followingConnection.startCoord.x != followingConnection.destinationCoord.x && this.lastFollowedConnection.startCoord.x == this.lastFollowedConnection.destinationCoord.x) || (followingConnection.startCoord.y != followingConnection.destinationCoord.y && this.lastFollowedConnection.startCoord.y == this.lastFollowedConnection.destinationCoord.y)))
                {
                    base.mainBodyChunk.vel *= 0.7f;
                    base.bodyChunks[1].vel *= 0.5f;
                }
                if (followingConnection.type == MovementConnection.MovementType.DropToFloor)
                {
                    this.footingCounter = 0;
                }
                this.MoveTowards(vector);
            }
            this.lastFollowedConnection = followingConnection;
        }



        public override Color ShortCutColor()
        {
            try
            {
                return this.ivars.basecolor.rgb;
            }
            catch(Exception e)
            {
                Debug.Log(":[");
                return new(0f, 0.5f, 1f);
            }
        }
        public struct IndividualVariations
        {
            public IndividualVariations(HSLColor basecolor, HSLColor effectcolor)
            {
                this.basecolor = basecolor;
                this.effectcolor = effectcolor;
            }
            public HSLColor basecolor;
            public HSLColor effectcolor;
        }

        public IndividualVariations ivars;
        public void GenerateIVars()
        {
            Random.State state = Random.state;
            Random.InitState(base.abstractCreature.ID.RandomSeed);
            float hue = Random.value;
            HSLColor bcolor = new HSLColor(hue, 1f, 0.6f);
            HSLColor ecolor = new HSLColor(hue + (Random.value * 10), 1f, 0.3f);
            ivars = new IndividualVariations(bcolor, ecolor);
            Random.state = state;
        }
        public SquabAI ai;
        public bool IsEdible(PhysicalObject otherObject)
        {
            if(otherObject is DangleFruit | otherObject is SlimeMold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public new SquabState State;
        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);
            if (IsEdible(otherObject))
            {
                this.Grab(otherObject, 0, 0, Grasp.Shareability.CanNotShare, 0.8f, false, false);
            }
        }

        public override void InitiateGraphicsModule()
        {
            if(base.graphicsModule == null)
            {
                base.graphicsModule = new Squabgraphics(this);
            }
            base.graphicsModule.Reset();
        }

        public override void Update(bool eu)
        {
            if(this.room.game.devToolsActive && Input.GetKey(bringMyLittleBoy))
            {
                base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + this.room.game.cameras[0].pos) * 14f;
                this.Stun(12);
            }
            /*
            if (!base.dead && this.State.health < 0f && Random.value < -this.State.health && Random.value < 0.025f)
            {
                this.Die();
            }
            if (!base.dead && Random.value * 0.7f > this.State.health && Random.value < 0.125f)
            {
                this.Stun(Random.Range(1, Random.Range(1, 27 - Custom.IntClamp((int)(20f * this.State.health), 0, 10))));
            }
            */
            base.Update(eu);
            if (base.Consious)
            {
                this.footingCounter++;
                this.Act();
            }
            else
            {
                this.footingCounter = 0;
            }
            if (this.Footing)
            {
                for (int i = 0; i < 2; i++)
                {
                    base.bodyChunks[i].vel *= 0.8f;
                    BodyChunk bodyChunk = base.bodyChunks[i];
                    bodyChunk.vel.y = bodyChunk.vel.y + base.gravity;
                }
            }
            if (base.Consious && !this.Footing && this.ai.behavior == SquabAI.Behavior.Flee && !base.safariControlled)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (this.room.aimap.TileAccessibleToCreature(this.room.GetTilePosition(base.bodyChunks[j].pos), base.Template))
                    {
                        base.bodyChunks[j].vel += Custom.DegToVec(Random.value * 360f) * Random.value * 5f;
                    }
                }
            }
        }
       public float runCycle = 2f;
        private int outOfWaterFooting;
    }

    internal class Squabgraphics : GraphicsModule
    {
        public BodyPart head;
        public BodyPart tail;
        public Limb[] limbs;

        public Squabgraphics(Squab squab) : base(squab,false)
        {
            head = new BodyPart(this);
            tail = new BodyPart(this);
            tail.rad = 1f;
            bodyParts = new BodyPart[4];
            limbs = new Limb[2];
            limbs[0] = new Limb(this, this.squab.bodyChunks[0], 0, 1f, 0.5f, 0.9f, 8f, 0.9f);
            limbs[1] = new Limb(this, this.squab.bodyChunks[0], 1, 1f, 0.5f, 0.9f, 8f, 0.9f);
            bodyParts[0] = limbs[0];
            bodyParts[1] = limbs[1];
            bodyParts[2] = this.head;
            bodyParts[3] = this.tail;
            Reset();

        }
        public Squab squab
        {
            get { return base.owner as Squab; }
        }
        public int TotalSprites = 4;

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Midground");

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                newContainer.AddChild(sLeaser.sprites[i]);
            }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            if (culled)
            {
                return;
            }
            Color fuckyou = new Color(1f, 1f, 0f);
            sLeaser.sprites[0].color = fuckyou;
            sLeaser.sprites[1].color = fuckyou;
            sLeaser.sprites[2].color = fuckyou;
            sLeaser.sprites[3].color = fuckyou;

            Vector2 headchunkpos = Vector2.Lerp(this.squab.bodyChunks[1].lastPos, this.squab.bodyChunks[1].pos, timeStacker);
            Vector2 bodychunkpos = Vector2.Lerp(this.squab.bodyChunks[0].lastPos, this.squab.bodyChunks[0].pos, timeStacker);
            Vector2 bodydir = Custom.DirVec(headchunkpos, bodychunkpos);
            sLeaser.sprites[1].x = head.pos.x;
            sLeaser.sprites[1].y = head.pos.y;
            sLeaser.sprites[0].x = bodychunkpos.x;
            sLeaser.sprites[0].y = bodychunkpos.y;
            sLeaser.sprites[2].x = tail.pos.x;
            sLeaser.sprites[2].y = tail.pos.y;
            sLeaser.sprites[3].x = limbs[0].pos.x;
            sLeaser.sprites[3].x = limbs[0].pos.y;
            
            ApplyPalette(sLeaser,rCam,rCam.currentPalette);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[this.TotalSprites];
            //when i actually make the custom sprites there will be an extra sprite for the gradient.
            //plus ill add on the googly eyes.
            sLeaser.sprites[0] = new FSprite("Circle20"); //chunky body
            sLeaser.sprites[1] = new FSprite("Circle4"); //head
            sLeaser.sprites[2] = new FSprite("Circle4"); //tail bud
            sLeaser.sprites[3] = new FSprite("LizardArm_13"); //leggy. only using 1 for now
            //sLeaser.sprites[4] = new FSprite("LizardArm_13"; //this is the other leggy.
            AddToContainer(sLeaser, rCam, null);
            base.InitiateSprites(sLeaser, rCam);
        }

        float runCycle;
        float lastRunCycle;
        public override void Update()
        {
            base.Update();
            this.lastRunCycle = this.runCycle;
            this.runCycle = this.squab.runCycle;
            head.Update();
            this.head.lastPos = this.head.pos;
            this.head.pos += this.head.vel;
            Vector2 vector = this.squab.mainBodyChunk.pos + Custom.DirVec(this.squab.bodyChunks[1].pos, this.squab.mainBodyChunk.pos);
            this.head.ConnectToPoint(vector, 4f, false, 0f, this.squab.mainBodyChunk.vel, 0.5f, 0.1f);
            this.head.vel += (vector - this.head.pos) / 6f;
            tail.Update();
            this.tail.lastPos = this.tail.pos;
            this.tail.pos += this.tail.vel;
            vector = this.squab.bodyChunks[1].pos + Custom.DirVec(this.squab.mainBodyChunk.pos, this.squab.bodyChunks[1].pos) * 8f;
            this.tail.ConnectToPoint(vector, 7f, false, 0f, this.squab.bodyChunks[1].vel, 0.1f, 0f);
            this.tail.vel += (vector - this.tail.pos) / 46f;
            this.tail.pos += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
            this.tail.PushOutOfTerrain(this.squab.room, this.squab.bodyChunks[1].pos);
            Vector2 vector2 = Custom.DirVec(this.squab.bodyChunks[1].pos, this.squab.bodyChunks[0].pos);
            Vector2 a2 = Custom.PerpendicularVector(vector2) * Mathf.Lerp(1f, -1f, 1.05f);
            this.limbs[0].Update();
            this.limbs[0].ConnectToPoint(this.squab.bodyChunks[0].pos, 12f, false, 0f, this.squab.bodyChunks[0].vel, 0f, 0f);
            Limb limb = this.limbs[0];
            limb.vel.y = limb.vel.y - 0.6f;
            float num = Mathf.Sin((this.runCycle + (float)this.limbs[0].limbNumber / 4f) * 3.1415927f * 2f);
            Vector2 goalPos = this.squab.bodyChunks[0].pos + vector2 * 8f * (0.3f + 0.7f * num) + a2 * 4f;
            this.limbs[0].FindGrip(this.squab.room, this.squab.bodyChunks[0].pos, this.squab.bodyChunks[0].pos, 15f, goalPos, 2, 2, false);
            this.limbs[0].pos += vector2 * (2f + num * 8f);
            this.limbs[0].pos -= a2 * 3f * Mathf.Cos((this.runCycle + (float)this.limbs[0].limbNumber / 4f) * 3.1415927f * 2f) * 1f;
        }
    }
}
