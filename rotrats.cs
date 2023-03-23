using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Sandbox;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace thelost
{

    public class rotratcritob : Critob
    {
        public rotratcritob() : base(CustomTemplates.Rotrat)
        {
            Icon = new SimpleIcon("Kill_Mouse", new Color(1f, 0.4f, 0f));
            LoadedPerformanceCost = 100f;
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
            RegisterUnlock(KillScore.Configurable(3), SandboxUnlockID.Rotrat);
            rrhooks hooks = new rrhooks();
            hooks.applyHooks();
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new MouseAI(acrit, acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new LanternMouse(acrit, acrit.world);
        }
        public override CreatureState CreateState(AbstractCreature acrit)
        {
            return new MouseState(acrit);
        }

        public override CreatureTemplate CreateTemplate()
        {
            CreatureTemplate t = new CreatureFormula(CreatureTemplate.Type.LanternMouse, CustomTemplates.Rotrat, "Rotrat").IntoTemplate();
            t.dangerousToPlayer = 1;
            t.grasps = 1;
            t.stowFoodInDen = true;
            t.shortcutColor = new Color(1f, 0.4f, 0f);

                return t;
        }
        public override IEnumerable<string> WorldFileAliases()
        {
            return new[] { "rotrat" };
        }
        public override void EstablishRelationships()
        {
            Relationships rels = new Relationships(CustomTemplates.Rotrat);
            rels.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, 1f);
            rels.Eats(CreatureTemplate.Type.JetFish,1f);
            rels.Eats(CreatureTemplate.Type.Scavenger, 1f);
            rels.Eats(CreatureTemplate.Type.EggBug, 1f);
            rels.Eats(CreatureTemplate.Type.SmallNeedleWorm, 1f);
            rels.Eats(CreatureTemplate.Type.SmallCentipede, 1f);
            rels.Eats(CreatureTemplate.Type.Hazer, 1f);
            rels.Eats(CreatureTemplate.Type.Spider, 1f);
            rels.Eats(CreatureTemplate.Type.VultureGrub, 1f);
            rels.Eats(CreatureTemplate.Type.TubeWorm, 1f);
            rels.Eats(CreatureTemplate.Type.Fly, 1f);
            if (ModManager.MSC)
            {
                rels.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC, 1f);
                rels.Eats(MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek, 1f);
            }

        }
    }

    public class rrhooks
    {
        public void applyHooks()
        {
            On.MouseAI.ctor += GivePreyTracker;
            On.MouseAI.Update += Hunter;
            On.LanternMouse.Update += LanternMouse_Update;
            On.LanternMouse.ctor += ivars;
            On.MouseAI.IUseARelationshipTracker_ModuleToTrackRelationship += Preyrelationshipfix;
            On.LanternMouse.CarryObject += fuckingcarrythingsplease;
        }

        private void fuckingcarrythingsplease(On.LanternMouse.orig_CarryObject orig, LanternMouse self)
        {
            if (!self.safariControlled && self.grasps[0].grabbed is Creature && self.AI.DynamicRelationship((self.grasps[0].grabbed as Creature).abstractCreature).type != CreatureTemplate.Relationship.Type.Eats) 
            {
                self.AI.preyTracker.ForgetPrey((self.grasps[0].grabbed as Creature).abstractCreature);
                self.LoseAllGrasps();
                return;   
            }
            PhysicalObject grabbed = self.grasps[0].grabbed;
            float num = self.mainBodyChunk.rad + self.grasps[0].grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad;
            Vector2 a = -Custom.DirVec(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos) * (num - Vector2.Distance(self.mainBodyChunk.pos, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos));
            float num2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass / (self.mainBodyChunk.mass + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass);
            num2 *= 0.2f * (1f - self.AI.stuckTracker.Utility());
            self.mainBodyChunk.pos += a * num2;
            self.mainBodyChunk.vel += a * num2;
            grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos -= a * (1f - num2);
            grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel -= a * (1f - num2);
            Vector2 vector = self.mainBodyChunk.pos + Custom.DirVec(self.bodyChunks[1].pos, self.mainBodyChunk.pos) * num;
            Vector2 vector2 = grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel - self.mainBodyChunk.vel;
            grabbed.bodyChunks[self.grasps[0].chunkGrabbed].vel = self.mainBodyChunk.vel;
            if (self.enteringShortCut == null && (vector2.magnitude * grabbed.bodyChunks[self.grasps[0].chunkGrabbed].mass > 30f || !Custom.DistLess(vector, grabbed.bodyChunks[self.grasps[0].chunkGrabbed].pos, 70f + grabbed.bodyChunks[self.grasps[0].chunkGrabbed].rad)))
            {
                self.LoseAllGrasps();
            }
            else
            {
                grabbed.bodyChunks[self.grasps[0].chunkGrabbed].MoveFromOutsideMyUpdate(self.abstractCreature.world.game.evenUpdate, vector);
            }
            if (self.grasps[0] != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    self.grasps[0].grabbed.PushOutOf(self.bodyChunks[i].pos, self.bodyChunks[i].rad, self.grasps[0].chunkGrabbed);
                }
            }
        }

        private void corruptiondraw(On.MouseGraphics.orig_DrawSprites orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if(self.mouse.Template.type == CustomTemplates.Rotrat)
            {
                rotratdata.TryGetValue(self.mouse, out rotdata value);
                for (int i = 0; i < value.bulbs.Length; i++)
                {
                    sLeaser.sprites[value.currentsprite].color = self.DecalColor;
                    sLeaser.sprites[value.currentsprite].x = sLeaser.sprites[self.EyeASprite(1)].x;
                    sLeaser.sprites[value.currentsprite].y = sLeaser.sprites[self.EyeASprite(1)].y;
                    sLeaser.sprites[value.currentsprite + 1].x = sLeaser.sprites[value.currentsprite].x;
                    sLeaser.sprites[value.currentsprite + 1].y = sLeaser.sprites[value.currentsprite].y;
                    sLeaser.sprites[value.currentsprite + 1].color = self.mouse.iVars.color.rgb;
                    value.currentsprite += 2;
                }
            }
        }

        private void corruptioncontainer(On.MouseGraphics.orig_AddToContainer orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            if (self.mouse.Template.type == CustomTemplates.Rotrat)
            {
                if(rotratdata.TryGetValue(self.mouse,out rotdata value) && value.ready == true)
                {
                    FContainer container = rCam.ReturnFContainer("Midground");
                    for(int i = 0; i < value.numofsprites; i++)
                    {
                        FSprite bulb = sLeaser.sprites[value.startsprite + i];
                        container.AddChild(bulb);
                    }
                    value.ready = false;
                }
            }
            }

        private void rotsetup(On.MouseGraphics.orig_ctor orig, MouseGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if(self.mouse.Template.type == CustomTemplates.Rotrat)
            {
                rotdata data = new rotdata();
                int numbulbs = Random.Range(3, 7);
                data.bulbs = new bulb[numbulbs];
                data.numbulbs = numbulbs;
                for (int i = 0; i < data.numbulbs; i++)
                {
                    data.bulbs[i] = new bulb();
                }
                rotratdata.Add(self.mouse,data);
            }
        }

        private void CorruptionInit(On.MouseGraphics.orig_InitiateSprites orig, MouseGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if(self.mouse.Template.type == CustomTemplates.Rotrat)
            {
                rotratdata.TryGetValue(self.mouse, out rotdata value);
                value.startsprite = sLeaser.sprites.Length;
                value.numofsprites = value.bulbs.Length *2;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + value.numofsprites);
                value.currentsprite = value.startsprite;
                for (int i = 0; i < value.numbulbs; i ++)
                {
                    value.bulbs[i].firstsprite = value.currentsprite;
                        sLeaser.sprites[value.bulbs[i].firstsprite] = new FSprite("Futile_White", true);
                        sLeaser.sprites[value.bulbs[i].firstsprite].shader = rCam.room.game.rainWorld.Shaders["JaggedCircle"];
                        sLeaser.sprites[value.bulbs[i].firstsprite].scale = 0.5f;
                    sLeaser.sprites[value.bulbs[i].firstsprite + 1] = new FSprite("mouseEyeB5", true);
                    sLeaser.sprites[value.bulbs[i].firstsprite + 1].scale = 0.5f;
                    value.currentsprite += 2;
                }
                value.ready = true;
                value.currentsprite = value.startsprite;
                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        ConditionalWeakTable<LanternMouse, rotdata> rotratdata = new ConditionalWeakTable<LanternMouse, rotdata>();
        public class rotdata
        {
            public int startsprite;
            public int currentsprite;
            public int numofsprites;
            public int numbulbs;
            public bool ready;
            public bulb[] bulbs;
        }
        public class bulb
        {
            public int firstsprite;
            public int numofsprites = 2;
            
        }

        private AIModule Preyrelationshipfix(On.MouseAI.orig_IUseARelationshipTracker_ModuleToTrackRelationship orig, MouseAI self, CreatureTemplate.Relationship relationship)
        {
            if(relationship.type == CreatureTemplate.Relationship.Type.Eats)
            {
                return self.preyTracker;
            }
            return orig(self, relationship);
        }

        private void ivars(On.LanternMouse.orig_ctor orig, LanternMouse self, AbstractCreature abstractCreature, World world)
        {
           
            orig(self, abstractCreature, world);
            if(self.Template.type == CustomTemplates.Rotrat)
            {
                Random.State state = Random.state;
                Random.InitState(self.abstractCreature.ID.RandomSeed);
                float hue;
                if (Random.value < 0.01)
                {
                    hue = 0.8532407407407407f;
                    Debug.Log("the mouse behind the slaughter....");
                    // hehe purple mouse.
                }
                else
                            if (Random.value < 0.05)
                {
                    hue = Mathf.Lerp(0.444f, 0.527f, Random.value);
                    //shock cyans?
                }
                else if (Random.value < 0.2)
                {
                    hue = Mathf.Lerp(0f, 0.05f, Random.value);
                    //shock reds?
                }
                else
                {
                    hue = Mathf.Lerp(0.055f, 0.125f, Random.value);
                    //shock oranges + yellows?
                }
                HSLColor color = new HSLColor(hue, 1f, Random.Range(0.4f,0.8f));
                float value = Random.value;
                self.iVars = new LanternMouse.IndividualVariations(value, color);
                Random.state = state;
            }
        }

        private void LanternMouse_Update(On.LanternMouse.orig_Update orig, LanternMouse self, bool eu)
        {
            orig(self, eu);
           if(self.Template.type == CustomTemplates.Rotrat)
            {
                if(self.grasps[0] != null)
                {
                    self.CarryObject();
                }
                if (self.AI.behavior == MouseAI.Behavior.Hunt)
                {
                    if (self.AI.preyTracker.MostAttractivePrey != null)
                    {
                        Tracker.CreatureRepresentation prey = self.AI.preyTracker.MostAttractivePrey;
                        Creature realprey = prey.representedCreature.realizedCreature;
                        if (Custom.DistLess(prey.representedCreature.pos, self.abstractCreature.pos, 4f))
                        {
                            self.Squeak(1f);
                            if (self.grasps[0] == null && (realprey.dead || realprey.Stunned))
                            {
                                self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, 0.5f, false, true);
                                self.AI.behavior = MouseAI.Behavior.ReturnPrey;
                            }
                            else
                            {
                                if(realprey.TotalMass < self.TotalMass*1.5)
                                {
                                    realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));
                                    self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, Random.Range(0.3f, 0.7f), true, true);
                                    
                                }
                                else
                                {
                                    if(Random.Range(0f,100f) < 20f)
                                    {

                                        realprey.Violence(self.mainBodyChunk, Custom.DirVec(self.mainBodyChunk.pos, realprey.mainBodyChunk.pos), realprey.mainBodyChunk, null, Creature.DamageType.Bite, Random.Range(0.6f, 1.4f), Random.Range(0.2f, 1.2f));

                                    }
                                    else
                                    {
                                        realprey.Stun(realprey.stun);
                                    }
                                    self.Grab(prey.representedCreature.realizedObject, 0, 0, Creature.Grasp.Shareability.CanNotShare, Random.Range(0.3f, 0.7f), true, false);
                                }
                                
                            }
                        }
                    }
                }
            }
        }

        private void Hunter(On.MouseAI.orig_Update orig, MouseAI self)
        {
            
            
            if(self.mouse.Template.type == CustomTemplates.Rotrat)
            {
                self.preyTracker.Update();
                self.stuckTracker.Update();
                orig(self);
                AIModule aimoduule = self.utilityComparer.HighestUtilityModule();
                if (aimoduule != null && aimoduule is PreyTracker)
                {
                    self.behavior = MouseAI.Behavior.Hunt;
                }
                if (self.behavior == MouseAI.Behavior.Hunt)
                {
                    if (self.mouse.grasps[0] != null && self.mouse.grasps[0].grabbed is Creature && self.StaticRelationship((self.mouse.grasps[0].grabbed as Creature).abstractCreature).type == CreatureTemplate.Relationship.Type.Eats)
                    {
                        self.behavior = MouseAI.Behavior.ReturnPrey;
                    }
                    else if (self.preyTracker.MostAttractivePrey != null && !self.mouse.safariControlled)
                    {
                        self.creature.abstractAI.SetDestination(self.preyTracker.MostAttractivePrey.BestGuessForPosition());
                        self.mouse.runSpeed = Mathf.Lerp(self.mouse.runSpeed, 1f, 0.08f);
                    }
                }
                if (self.behavior == MouseAI.Behavior.ReturnPrey)
                {
                    if (self.denFinder.GetDenPosition() != null)
                    {
                        self.creature.abstractAI.SetDestination(self.denFinder.GetDenPosition().Value);
                        Debug.Log($"rorat number {self.mouse.abstractCreature.ID.number.ToString()}: YIPPE! i found a den!");
                    }
                    else
                    {
                        Debug.Log($"rorat number {self.mouse.abstractCreature.ID.number.ToString()}: FUCK! no den found :[");
                    }
                }
                if (Input.GetKey(KeyCode.T) && Input.GetKey(KeyCode.M))
                {
                    self.behavior = MouseAI.Behavior.Hunt;
                    Debug.Log("RRs have been forced to hunt.");
                }
            }
            else
            {
                orig(self);
            }
        }

        private void GivePreyTracker(On.MouseAI.orig_ctor orig, MouseAI self, AbstractCreature creature, World world)
        {
            orig(self, creature, world);
            if(self.mouse.Template.type == CustomTemplates.Rotrat)
            {
                self.AddModule(new PreyTracker(self, 3, 2f, 10f, 70f, 0.5f));
                self.utilityComparer.AddComparedModule(self.preyTracker, null, 1f, 1.5f);
                self.AddModule(new StuckTracker(self, true, false));
            }
            
        }
    }
}
