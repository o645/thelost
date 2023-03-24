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
        SquabAI.Behavior behavior;
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
        }
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
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            
                sLeaser.RemoveAllSpritesFromContainer();
                if (newContatiner == null)
                {
                    newContatiner = rCam.ReturnFContainer("Midground");
                }
                for (int i = 0; i < this.TotalSprites; i++)
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                }
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (culled)
            {
                return;
            }
            sLeaser.sprites[0].color = this.squab.ivars.basecolor.rgb;
            sLeaser.sprites[1].color = this.squab.ivars.effectcolor.rgb;
            sLeaser.sprites[2].color = this.squab.ivars.basecolor.rgb;
            sLeaser.sprites[3].color = this.squab.ivars.effectcolor.rgb;

            Vector2 headchunkpos = Vector2.Lerp(this.squab.bodyChunks[1].lastPos, this.squab.bodyChunks[1].pos, timeStacker);
            Vector2 bodychunkpos = Vector2.Lerp(this.squab.bodyChunks[0].lastPos, this.squab.bodyChunks[0].pos, timeStacker);
            Vector2 bodydir = Custom.DirVec(headchunkpos, bodychunkpos);
            Vector2 tailspritepos = bodychunkpos + new Vector2(1.5f, 0.5f);
            Vector2 headspritepos = bodychunkpos + new Vector2(-1f, 1f);
            sLeaser.sprites[1].x = headspritepos.x;
            sLeaser.sprites[1].y = headspritepos.y;
            sLeaser.sprites[0].x = bodychunkpos.x;
            sLeaser.sprites[0].y = bodychunkpos.y;
            sLeaser.sprites[2].x = tailspritepos.x;
            sLeaser.sprites[2].y = tailspritepos.y;
            sLeaser.sprites[3].x = bodychunkpos.x + 1f;
            sLeaser.sprites[3].x = bodychunkpos.y - 1f;
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
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

        public override void Update()
        {
            base.Update();
            head.Update();
            tail.Update();
        }
    }
}
