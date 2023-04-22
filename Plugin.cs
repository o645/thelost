using BepInEx;
using Fisobs.Core;
using Menu.Remix.MixedUI;
using MonoMod.RuntimeDetour;
using RWCustom;
using SlugBase.Features;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using static SlugBase.Features.FeatureTypes;
using Random = UnityEngine.Random;


#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete
namespace thelost
{
    [BepInPlugin("thelost", "The Lost", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        private OptionsMenu options;

        private void Awake()
        {

            // Plugin startup logic
            Logger.LogInfo("miros cat when?");

            On.RainWorld.OnModsInit += init;
            On.Centipede.Shock += ResistCentiShocks;
            On.Player.Update += HissingCommand;
            On.Player.Update += ShockCommand;
            On.SporeCloud.Update += puffStun;
            On.Player.Update += LCcaller; // very not finished!
            try
            {
                Hook overseercolorhook = new Hook(typeof(OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Plugin).GetMethod("OverseerGraphics_MainColor_get", BindingFlags.Static | BindingFlags.Public));
                Hook overseerNeutralHook = new Hook(typeof(OverseerGraphics).GetProperty("NeutralColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Plugin).GetMethod("OverseerGraphics_NeutralColor_get", BindingFlags.Static | BindingFlags.Public));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            Content.Register(new mintcritob());
            Content.Register(new rotratcritob());
            try
            {
                Content.Register(new Squabcritob());
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
            ConversationOverrides.Hooks();
            WhiskerStuff.Hooks();

        }




        public delegate Color orig_OverseerMainColor(OverseerGraphics self);
        public static Color OverseerGraphics_MainColor_get(orig_OverseerMainColor orig, OverseerGraphics self)
        {
            Color res = orig(self);
            if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 69)
            {
                res = new Color(1f, 0.5f, 0.1f);
            }
            return res;
        }
        public delegate Color orig_OverseerNeutralColor(OverseerGraphics self);
        public static Color OverseerGraphics_NeutralColor_get(orig_OverseerNeutralColor orig, OverseerGraphics self)
        {
            Color res = orig(self);
            if ((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 69)
            {
                res = new Color(1f, 0.584f, 0.109f);
            }
            return res;
        }
        public Plugin()
        {
            options = new OptionsMenu(this);
        }
        private void init(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                MachineConnector.SetRegisteredOI("thelost", options);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                Logger.LogMessage("WHOOPS");
            }
        }



        public List<string> currentDialog = new List<string>();
        public bool Speaking = false;
        AbstractCreature LCOverseer;

        int lccooldown = 0;
        private void LCcaller(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (Input.GetKey(KeyCode.F) && lccooldown == 0 && self.room != null)
            {

                lccooldown = 400;
                self.room.AddObject(new NeuronSpark(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y + 40f)));

                if (Speaking == false)
                {

                    WorldCoordinate worldC = new WorldCoordinate(self.room.world.offScreenDen.index, -1, -1, 0);
                    LCOverseer = new AbstractCreature(self.room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Overseer), null, worldC, new EntityID(-1, 5));
                    self.room.world.GetAbstractRoom(worldC).entitiesInDens.Add(LCOverseer);
                    LCOverseer.ignoreCycle = true;
                    (LCOverseer.abstractAI as OverseerAbstractAI).spearmasterLockedOverseer = true;
                    (LCOverseer.abstractAI as OverseerAbstractAI).SetAsPlayerGuide(69);
                    (LCOverseer.abstractAI as OverseerAbstractAI).BringToRoomAndGuidePlayer(self.room.abstractRoom.index);
                    Logger.LogInfo("Called overseer. now trying to converse!");
                    self.room.game.cameras[0].hud.InitDialogBox();
                    Debug.Log("init'd dialog box");
                    int rng = Random.Range(0, 3);
                    switch (rng)
                    {
                        case 0:
                            currentDialog.Add("Hello there, little one!");
                            break;
                        case 1:
                            currentDialog.Add("You called?");
                            break;
                        case 2:
                            currentDialog.Add("What is it, little one?");
                            break;
                        default:
                            currentDialog.Add("Yes, little one?");
                            break;
                    }

                    if (self.grasps[0] != null)
                    {
                        AbstractPhysicalObject.AbstractObjectType item = self.grasps[0].grabbed.abstractPhysicalObject.type;
                        if (item == AbstractPhysicalObject.AbstractObjectType.Spear)
                        {
                           
                            currentDialog.Add("Oh! Looks to be a sharpened piece of rebar.");
                            currentDialog.Add("I suppose this is broken off of the buildings nearby little one?");

                        }
                        else if (item == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
                        {
                            
                            currentDialog.Add("Oh, be careful with that little one!");
                            currentDialog.Add("If you aren't careful, you might cause it to explode!");
                            currentDialog.Add("As fun as that sounds, it would not be so fun for you~");
                        }
                        else if (item == AbstractPhysicalObject.AbstractObjectType.Rock)
                        {
                            currentDialog.Add("Hmm? Apologises little one, but there isn't much i can say about this!");
                            currentDialog.Add("It is just a simple piece of rubbish.");

                        }
                        else if (item == AbstractPhysicalObject.AbstractObjectType.DangleFruit)
                        {
                            
                            currentDialog.Add("Oh! This is a bluefruit! Not particularly filling for you, but it'll do in a pinch!");
                        }
                        else if (item == MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType.JokeRifle)
                        {
                            (LCOverseer.realizedCreature as Overseer).TryAddHologram(OverseerHolograms.OverseerHologram.Message.DangerousCreature, self, 1f);
                            currentDialog.Add("... Lost one, how in the world did you get that?");
                        }
                        else
                        {
                            currentDialog.Add("Sorry little one, I can't quite make out what that is...");
                            
                        }

                    }
                    else
                    {
                        Region region = self.room.abstractRoom.world.region;
                        if (region.name == "UW" && self.room.abstractRoom.subregionName == "Underhang")
                        {
                            currentDialog.Add("... That's a rather high drop!");
                            currentDialog.Add("Little one, you certainly wouldn't survive it!~");
                            currentDialog.Add("The air here is so full of static... You'll be safe from the rain, but I wouldn't suggest staying out too long!");
                        }
                        else
                        {
                            //generic dialog here.
                            currentDialog.Add("Are you lost again?");
                        }
                    }
                    Speaking = true;
                }

            }
            if (Speaking == true)
            {
                        if (currentDialog.Count == 0)
                        {
                            Speaking= false;
                        }
                        else
                        {
                            HUD.DialogBox dialogbox = self.room.game.cameras[0].hud.dialogBox;
                            dialogbox.NewMessage(currentDialog[0], 10);
                            currentDialog.Remove(currentDialog[0]);
                    
                        }
            }
            if (lccooldown > 0)
            {
                if (lccooldown > 350)
                {
                    (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (-0.01f);
                    self.room.AddObject(new NeuronSpark(new Vector2(self.bodyChunks[0].pos.x, self.bodyChunks[0].pos.y + 40f)));
                }
                lccooldown--;
            }
        }

     
        void ResistCentiShocks(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
        {


            if (shockObj is Player scuggy)
            {


                if (scuggy.slugcatStats.name.value == "thelost")
                {
                    if (self.graphicsModule != null)
                    {
                        (self.graphicsModule as CentipedeGraphics).lightFlash = 1f;
                        for (int i = 0; i < (int)Mathf.Lerp(4f, 8f, self.size); i++)
                        {
                            self.room.AddObject(new Spark(self.HeadChunk.pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
                        }
                    }
                    self.room.PlaySound(SoundID.Centipede_Shock, self.HeadChunk.pos, 1f, 1f);
                    self.shockGiveUpCounter = Math.Max(self.shockGiveUpCounter, 30);
                }
                else
                {
                    orig(self, shockObj);
                }
            }
            else
            {
                orig(self, shockObj);
            }

        }

        void HissingCommand(On.Player.orig_Update orig, Player self, bool eu)
        {

            orig(self, eu);

            if (self.room != null)
            {
                if (hissCooldownTimer[self.playerState.playerNumber] >= 100)
                {
                    self.Blink(10);
                    self.bodyChunks[0].vel += Custom.RNV() * ((hissCooldownTimer[self.playerState.playerNumber] / 100) / -80f);
                }
                if (hissCooldownTimer[self.playerState.playerNumber] > 0)
                {
                    hissCooldownTimer[self.playerState.playerNumber]--;
                }
                if (Input.GetKeyDown(options.deerbind.Value) && self.slugcatStats.name.value == "thelost" && self.FoodInStomach >= options.deerpips.Value)
                {
                    if (hissCooldownTimer[self.playerState.playerNumber] == 0)
                    {
                        self.SubtractFood(options.deerpips.Value);
                        self.room.PlaySound(SoundID.In_Room_Deer_Summoned, self.bodyChunks[0].pos, 1f, 1f);
                        self.room.InGameNoise(new Noise.InGameNoise(self.bodyChunks[0].pos, 10f, self, 1f));
                        hissCooldownTimer[self.playerState.playerNumber] = 150;
                        if (self.slugcatStats.name.value == "thelost")
                        {
                            Debug.Log("Trying to call a deer");
                            List<AbstractCreature> list = new List<AbstractCreature>();
                            for (int l = 0; l < self.room.abstractRoom.creatures.Count; l++)
                            {
                                if (self.room.abstractRoom.creatures[l].creatureTemplate.type == CreatureTemplate.Type.Deer
                                    && self.room.abstractRoom.creatures[l].realizedCreature != null
                                    && self.room.abstractRoom.creatures[l].realizedCreature.Consious
                                    && (self.room.abstractRoom.creatures[l].realizedCreature as Deer).AI.goToPuffBall == null
                                    && (self.room.abstractRoom.creatures[l].realizedCreature as Deer).AI.pathFinder.CoordinateReachableAndGetbackable(new WorldCoordinate(self.coord.room, self.coord.x + 3, self.coord.y + 10, self.coord.abstractNode)))
                                {
                                    list.Add(self.room.abstractRoom.creatures[l]);
                                    Debug.Log("ayo deer found in room!");
                                }
                            }
                            if (list.Count > 0)
                            {
                                if (UnityEngine.Random.value < 0.7f)
                                {
                                    (list[UnityEngine.Random.Range(0, list.Count)].abstractAI as DeerAbstractAI).AttractToSporeCloud(self.coord);
                                    Debug.Log("A DEER IN THE ROOM WAS ATTRACTED!");
                                }
                            }
                        }
                    }

                }
            }


        }
        private int[] hissCooldownTimer = { 0, 0, 0, 0 };
        void ShockCommand(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);

            if (self.slugcatStats.name.value == "thelost")
            {
                if (Input.GetKeyDown(options.shockbind.Value) && self.grabbedBy.Count > 0 && !self.dead && self.FoodInStomach >= options.shockpips.Value)
                {
                    self.SubtractFood(options.shockpips.Value);
                    Creature shockObject = self.grabbedBy[0].grabber;
                    for (int i = 0; i < (int)Mathf.Lerp(4f, 8f, 6f); i++)
                    {
                        self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(1f, 0.7f, 0f), null, 8, 14));

                    }
                    shockObject.Violence(shockObject.mainBodyChunk, new Vector2?(new Vector2(0f, 0f)), shockObject.mainBodyChunk, null, Creature.DamageType.Electric, 2f, 200f);

                    self.room.AddObject(new CreatureSpasmer(shockObject, false, shockObject.stun));
                    self.room.PlaySound(SoundID.Centipede_Shock, self.bodyChunks[0].pos, 1f, 1f);

                    shockObject.LoseAllGrasps();
                    if (shockObject.Submersion > 0f)
                    {
                        self.room.AddObject(new UnderwaterShock(self.room, self, self.bodyChunks[0].pos, 14, Mathf.Lerp(ModManager.MMF ? 0f : 200f, 1200f, 6f), 0.2f + 1.9f * 6f, self, new Color(0.7f, 0.7f, 1f)));
                    }
                }
                if (Input.GetKeyDown(options.shockbind.Value) && self.grasps[0] != null && self.grasps[0].grabbed is Creature && self.FoodInStomach >= options.shockpips.Value)
                {
                    self.SubtractFood(options.shockpips.Value);
                    Creature shockObject = self.grasps[0].grabbed as Creature;
                    for (int i = 0; i < (int)Mathf.Lerp(4f, 8f, 6f); i++)
                    {
                        self.room.AddObject(new Spark(self.bodyChunks[0].pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(1f, 0.7f, 0f), null, 8, 14));

                    }
                    shockObject.Violence(shockObject.mainBodyChunk, new Vector2?(new Vector2(0f, 0f)), shockObject.mainBodyChunk, null, Creature.DamageType.Electric, 2f, 200f);

                    self.room.AddObject(new CreatureSpasmer(shockObject, false, shockObject.stun));
                    self.room.PlaySound(SoundID.Centipede_Shock, self.bodyChunks[0].pos, 1f, 1f);
                    self.grasps[0].Release();
                }
            }

        }

        void puffStun(On.SporeCloud.orig_Update orig, SporeCloud self, bool eu)
        {
            orig(self, eu);
            for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
            {
                if (self.room.abstractRoom.creatures[i].realizedCreature != null)
                {
                    if (self.room.abstractRoom.creatures[i].realizedCreature is Player scug)
                    {
                        if (scug.slugcatStats.name.value == "thelost")
                        {
                            if (Custom.DistLess(self.pos, self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, self.rad + self.room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.rad + 20f))
                            {
                                scug.Stun(Random.Range(10, 80));
                            }
                        }

                    }
                }
            }
        }
    }

    public class OptionsMenu : OptionInterface
    {
        public readonly Configurable<int> shockpips;
        public readonly Configurable<KeyCode> shockbind;
        public readonly Configurable<int> deerpips;
        public readonly Configurable<KeyCode> deerbind;


        private UIelement[] UIArrPlayerOptions;
        public OptionsMenu(Plugin plugin)
        {
            shockpips = this.config.Bind<int>("shockpips", 2);
            shockbind = this.config.Bind<KeyCode>("shockbind", KeyCode.S);
            deerpips = this.config.Bind<int>("deerpips", 1);
            deerbind = this.config.Bind<KeyCode>("deerbind", KeyCode.D);

        }
        public override void Initialize()
        {
            var opTab = new OpTab(this, "Options");
            this.Tabs = new[]
            {
                opTab
            };
            UIArrPlayerOptions = new UIelement[]
            {
                new OpLabel(10f,550f,"Options",true),
                new OpLabel(10f,520f,"Pips used for shocking"),
                new OpSlider(shockpips,new Vector2(10f,490f),5f),
                new OpLabel(10f,460f,"Bind for shocking"),
                new OpKeyBinder(shockbind,new Vector2(10f,430f),new Vector2(30f,30f),true,OpKeyBinder.BindController.Controller1),
                new OpLabel(10f,400f,"Pips used for deercalling"),
                new OpSlider(deerpips,new Vector2(10f,370f),5f),
                new OpLabel(10f,340f,"Bind for deercalling"),
                new OpKeyBinder(deerbind,new Vector2(10f,310f),new Vector2(30f,30f))
            };
            (UIArrPlayerOptions[2] as OpSlider).max = 6;
            (UIArrPlayerOptions[6] as OpSlider).max = 6;
            opTab.AddItems(UIArrPlayerOptions);
        }
    }
}