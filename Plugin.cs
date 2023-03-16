using BepInEx;

using static SlugBase.Features.FeatureTypes;
using UnityEngine;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;
using System.Security.Permissions;
using System.Security;
using Menu.Remix.MixedUI;
using Fisobs.Core;
using System.Reflection;
using MonoMod.RuntimeDetour;
using SlugBase.Features;
using MoreSlugcats;


#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete
namespace thelost
{
    [BepInPlugin("thelost", "The Lost", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        static readonly PlayerFeature<bool> ShockResist = PlayerBool("shock_resist");
        static readonly PlayerFeature<bool> Hissing = PlayerBool("lizardhiss");
        static readonly PlayerFeature<bool> ShockEnemy = PlayerBool("shockattack");
        static readonly PlayerFeature<bool> PuffHurts = PlayerBool("sporepuffs_hurt");
        static readonly PlayerFeature<bool> deercaller = PlayerBool("deercaller");
        static readonly PlayerFeature<bool> gilled = PlayerBool("gilled");
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
            On.PlayerGraphics.DrawSprites += whiskerDraw;
            On.PlayerGraphics.AddToContainer += whiskercontainer;
            On.PlayerGraphics.InitiateSprites += funnytailwhiskerinit;
            On.Player.ctor += addondata;
            //On.Player.Update += LCcaller; // very not finished!
            On.PlayerGraphics.ctor += whiskersetup;
            On.PlayerGraphics.Update += whiskerupdate;
            try
            {
                Hook overseercolorhook = new Hook(typeof(OverseerGraphics).GetProperty("MainColor", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), typeof(Plugin).GetMethod("OverseerGraphics_MainColor_get", BindingFlags.Static | BindingFlags.Public));
            }catch(Exception ex)
            {
                Logger.LogError(ex);
            }
            try
            {
                Content.Register(new mintcritob());
                Content.Register(new rotratcritob());
            } catch(Exception ex)
            {
                Logger.LogError(ex);
            }

        }


        public delegate Color orig_OverseerMainColor(OverseerGraphics self);
        public static Color OverseerGraphics_MainColor_get(orig_OverseerMainColor orig, OverseerGraphics self)
        {
            Color res = orig(self);
            if((self.overseer.abstractCreature.abstractAI as OverseerAbstractAI).ownerIterator == 69)
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

        private void whiskerupdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if(gilled.TryGet(self.player,out bool value) && value && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data))
            {
                int index = 0;
                for (int i = 0; i < 2; i++)
                {
                    for(int j = 0; j<3; j++)
                    {
                        Vector2 pos = self.tail[2].pos;
                        Vector2 pos2 = self.tail[3].pos;
                        float num = 0f;
                        float num2 = 90f;
                        int num3 = index % (data.tailScales.Length / 2);
                        float num4 = num2 / (float)(data.tailScales.Length/2);
                        if (i == 1)
                        {
                            pos.x += 5f;
                        }
                        else
                        {
                            pos.x -= 5f;
                        }
                        Vector2 a = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num + 90f);
                        float f = Custom.VecToDeg(self.lookDirection);
                        Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num);
                        Vector2 a2 = Vector2.Lerp(vector, Custom.DirVec(pos2, pos), Mathf.Abs(f));
                        if (data.tailpositions[index].y < 0.2f)
                        {
                            a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, data.tailpositions[index].y), 2f) * 2f;
                        }
                        a2 = Vector2.Lerp(a2, vector, Mathf.Pow(0.0875f, 1f)).normalized;
                        Vector2 vector2 = pos + a2 * data.tailScales.Length;
                        if (!Custom.DistLess(data.tailScales[index].pos, vector2, data.tailScales[index].length / 2f))
                        {
                            Vector2 a3 = Custom.DirVec(data.tailScales[index].pos, vector2);
                            float num5 = Vector2.Distance(data.tailScales[index].pos, vector2);
                            float num6 = data.tailScales[index].length / 2f;
                            data.tailScales[index].pos += a3 * (num5 - num6);
                            data.tailScales[index].vel += a3 * (num5 - num6);
                        }
                        data.tailScales[index].vel += Vector2.ClampMagnitude(vector2 - data.tailScales[index].pos, 10f) / Mathf.Lerp(5f, 1.5f, 0.5873646f);
                        data.tailScales[index].vel *= Mathf.Lerp(1f, 0.8f, 0.5873646f);
                        data.tailScales[index].ConnectToPoint(pos, data.tailScales[index].length, true, 0f, new Vector2(0f, 0f), 0f, 0f);
                        data.tailScales[index].Update();
                        index++;
                    }
                }
                index = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 pos = self.owner.bodyChunks[0].pos;
                        Vector2 pos2 = self.owner.bodyChunks[1].pos;
                        float num = 0f;
                        float num2 = 90f;
                        int num3 = index % (data.headScales.Length / 2);
                        float num4 = num2 / (float)(data.headScales.Length / 2);
                        if (i == 1)
                        {
                            num = 0f;
                            pos.x += 5f;
                        }
                        else
                        {
                            pos.x -= 5f;
                        }
                        Vector2 a = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num + 90f);
                        float f = Custom.VecToDeg(self.lookDirection);
                        Vector2 vector = Custom.rotateVectorDeg(Custom.DegToVec(0f), (float)num3 * num4 - num2 / 2f + num);
                        Vector2 a2 = Vector2.Lerp(vector, Custom.DirVec(pos2, pos), Mathf.Abs(f));
                        if (data.headpositions[index].y < 0.2f)
                        {
                            a2 -= a * Mathf.Pow(Mathf.InverseLerp(0.2f, 0f, data.headpositions[index].y), 2f) * 2f;
                        }
                        a2 = Vector2.Lerp(a2, vector, Mathf.Pow(0.0875f, 1f)).normalized;
                        Vector2 vector2 = pos + a2 * data.headScales.Length;
                        if (!Custom.DistLess(data.headScales[index].pos, vector2, data.headScales[index].length / 2f))
                        {
                            Vector2 a3 = Custom.DirVec(data.headScales[index].pos, vector2);
                            float num5 = Vector2.Distance(data.headScales[index].pos, vector2);
                            float num6 = data.headScales[index].length / 2f;
                            data.headScales[index].pos += a3 * (num5 - num6);
                            data.headScales[index].vel += a3 * (num5 - num6);
                        }
                        data.headScales[index].vel += Vector2.ClampMagnitude(vector2 - data.headScales[index].pos, 10f) / Mathf.Lerp(5f, 1.5f, 0.5873646f);
                        data.headScales[index].vel *= Mathf.Lerp(1f, 0.8f, 0.5873646f);
                        data.headScales[index].ConnectToPoint(pos, data.headScales[index].length, true, 0f, new Vector2(0f, 0f), 0f, 0f);
                        data.headScales[index].Update();
                        index++;
                    }
                }
            }
        }

        private void whiskersetup(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if (gilled.TryGet(self.player, out bool value) && value)
            {
                tailwhiskerstorage.Add(self.player, new Whiskerdata(self.player));
                tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data);
                for (int i = 0; i < data.tailScales.Length; i++)
                {
                    data.tailScales[i] = new Whiskerdata.Scale(self);
                    data.tailpositions[i] = new Vector2((i<data.tailScales.Length/2 ? 0.7f : -0.7f),0.28f);

                }
                for(int i = 0; i < data.headScales.Length; i++)
                {
                    data.headScales[i] = new Whiskerdata.Scale(self);
                    data.headpositions[i] = new Vector2((i < data.headScales.Length / 2 ? 0.7f : -0.7f), i==1 ? 0.035f : 0.026f);
                    
                }


            }
        }


        public List<string> currentDialog = new List<string>();
        public bool Speaking = false;
        AbstractCreature LCOverseer;

        int lccooldown = 0;
        private void LCcaller(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            Player.InputPackage inputPackage = RWInput.PlayerInput(self.playerState.playerNumber, self.room.game.rainWorld);
            if(inputPackage.mp && inputPackage.jmp && lccooldown == 0)
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
                            currentDialog.Add("... Lost one, how in the world did you get that?");
                            }
                            else
                            {
                            currentDialog.Add("Sorry little one, my creator hasn't programmed this item into me.");
                            currentDialog.Add("Probably too distracted thinking about poleplant cat...");
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
            if(Speaking == true)
            {
                if(LCOverseer != null && LCOverseer.realizedCreature != null)
                {
                    if (LCOverseer.realizedCreature?.room == self.room)
                    {
                        if (currentDialog.Count == 0)
                        {
                            LCOverseer.Destroy();
                        }
                        else
                        {
                            HUD.DialogBox dialogbox = self.room.game.cameras[0].hud.dialogBox;
                            dialogbox.NewMessage(currentDialog[0], 10);
                            currentDialog.Remove(currentDialog[0]);
                        }
                    }
                }
            }
            if(lccooldown > 0)
            {
                if(lccooldown > 350)
                {
                    (self.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * (0.8f / -80f);
                }
                lccooldown--;
            }
        }
        private void addondata(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);


        }

        private void funnytailwhiskerinit(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (gilled.TryGet(self.owner as Player,out bool gills) && gills)
            {
                
                tailwhiskerstorage.TryGetValue(self.player, out var thedata);
                thedata.initialtailwhiskerloc = sLeaser.sprites.Length;
                thedata.initialfacewhiskerloc = sLeaser.sprites.Length + 6;
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 6 + 4);
                // 6 for tail sprites, 4 for face sprites.
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sLeaser.sprites[thedata.tailwhiskersprite(i, j)] = new FSprite(thedata.sprite);
                        sLeaser.sprites[thedata.tailwhiskersprite(i, j)].scaleY = 10f / Futile.atlasManager.GetElementWithName(thedata.sprite).sourcePixelSize.y;
                        sLeaser.sprites[thedata.tailwhiskersprite(i, j)].anchorY = 0.1f;
                    }
                }
                for(int i= 0; i < 2; i++)
                {
                    for(int j = 0; j<2; j++)
                    {
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)] = new FSprite(thedata.facesprite);
                       
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)].scaleY = 10f / Futile.atlasManager.GetElementWithName(thedata.sprite).sourcePixelSize.y;
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)].anchorY = 0.1f;
                    }
                }
                self.AddToContainer(sLeaser, rCam, null);
            }
        }

        void ResistCentiShocks(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
        {


            if (shockObj is Player)
            {


                if (ShockResist.TryGet((shockObj as Player), out bool flag) && flag)
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
            
            if(self.room != null)
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
                if (Input.GetKeyDown(options.deerbind.Value)  && Hissing.TryGet(self, out bool flaghiss) && flaghiss && self.FoodInStomach >= options.deerpips.Value)
                {
                    if (hissCooldownTimer[self.playerState.playerNumber] == 0)
                    {
                        self.SubtractFood(options.deerpips.Value);
                        self.room.PlaySound(SoundID.In_Room_Deer_Summoned, self.bodyChunks[0].pos, 1f, 1f);
                        self.room.InGameNoise(new Noise.InGameNoise(self.bodyChunks[0].pos, 10f, self, 1f));
                        hissCooldownTimer[self.playerState.playerNumber] = 150;
                        if (deercaller.TryGet(self, out bool flagdeer) && flagdeer)
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
            
            if (ShockEnemy.TryGet(self, out bool flagshock) && flagshock)
            {
                if ( Input.GetKeyDown(options.shockbind.Value) && self.grabbedBy.Count > 0 && !self.dead && self.FoodInStomach >= options.shockpips.Value)
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
                    if (self.room.abstractRoom.creatures[i].realizedCreature is Player)
                    {
                        Creature scug = self.room.abstractRoom.creatures[i].realizedCreature;
                        if (PuffHurts.TryGet(scug as Player, out bool value))
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

       

        void whiskercontainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);
            
            if (gilled.TryGet(self.player, out bool gillval) && gillval && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data) && sLeaser.sprites.Length > 13)
            {
                FContainer container = rCam.ReturnFContainer("Midground");
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        FSprite whisker = sLeaser.sprites[data.tailwhiskersprite(i, j)];
                        container.AddChild(whisker);
                        if (i == 0)
                        {
                            whisker.MoveBehindOtherNode(sLeaser.sprites[2]);
                            whisker.MoveToBack();

                        }
                        else
                        {
                            whisker.MoveInFrontOfOtherNode(sLeaser.sprites[2]);

                        }

                    }
                }
                for(int i = 0; i < 2; i++)
                {
                    for(int j = 0; j < 2; j++)
                    {
                        FSprite whisker = sLeaser.sprites[data.facewhiskersprite(i, j)];
                        container.AddChild(whisker);
                    }
                }
            }

        }
        void whiskerDraw(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (gilled.TryGet(self.player, out bool gillval) && gillval && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data))
            {
                //oh god i need to rewrite all of this into one for loop.
                int index = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Vector2 vector = new Vector2((sLeaser.sprites[2] as TriangleMesh).vertices[10].x + camPos.x, (sLeaser.sprites[2] as TriangleMesh).vertices[10].y + camPos.y);
                        Vector2 lol = new Vector2((sLeaser.sprites[2] as TriangleMesh).vertices[12].x + camPos.x, (sLeaser.sprites[2] as TriangleMesh).vertices[12].y + camPos.y);
                        float f = 0f;
                        float num =  Custom.VecToDeg(Vector2.Lerp((sLeaser.sprites[2] as TriangleMesh).vertices[10], (sLeaser.sprites[2] as TriangleMesh).vertices[12],timeStacker));
                        float test = Vector2.SignedAngle(vector, lol);
                        if (i == 0)
                        {
                            vector.x -= 2f;
                        }
                        else
                        {
                            vector.x += 2f;
                        }
                        var spine = self.SpinePosition(0.8f, timeStacker);
                      
                        sLeaser.sprites[data.tailwhiskersprite(i, j)].x = vector.x - camPos.x;
                        sLeaser.sprites[data.tailwhiskersprite(i, j)].y = vector.y - camPos.y;
                        sLeaser.sprites[data.tailwhiskersprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(data.tailScales[index].lastPos, data.tailScales[index].pos, timeStacker)) + Custom.VecToDeg(spine.dir);
                        // if 
                        sLeaser.sprites[data.tailwhiskersprite(i, j)].scaleX = 0.75f * Mathf.Sign(f);
                        sLeaser.sprites[data.tailwhiskersprite(i, j)].color = sLeaser.sprites[1].color;
                        index++;
                    }
                }
                index = 0;
                for(int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        Vector2 vector = new Vector2(sLeaser.sprites[9].x + camPos.x, sLeaser.sprites[9].y + camPos.y);
                        float f = 0f;
                        float num = 0f;
                        if (i == 0)
                        {
                            vector.x -= 5f;
                        }
                        else
                        {
                            num = 180f;
                            vector.x += 5f;
                        }
                        sLeaser.sprites[data.facewhiskersprite(i, j)].x = vector.x - camPos.x;
                        sLeaser.sprites[data.facewhiskersprite(i, j)].y = vector.y - camPos.y;
                        sLeaser.sprites[data.facewhiskersprite(i, j)].rotation = Custom.AimFromOneVectorToAnother(vector, Vector2.Lerp(data.headScales[index].lastPos, data.headScales[index].pos, timeStacker)) + num;
                        sLeaser.sprites[data.facewhiskersprite(i, j)].scaleX = 0.4f * Mathf.Sign(f);
                        sLeaser.sprites[data.facewhiskersprite(i, j)].color = sLeaser.sprites[1].color;
                        index++;
                    }
                }

            }
        }


        public static ConditionalWeakTable<Player, Whiskerdata> tailwhiskerstorage = new ConditionalWeakTable<Player, Whiskerdata>();
        public class Whiskerdata
        {
            public bool ready = false;
            public int initialtailwhiskerloc;
            public int initialfacewhiskerloc;
            public string sprite = "LizardScaleA0";
            public string facesprite = "LizardScaleA0";
            public WeakReference<Player> playerref;
            public Whiskerdata(Player player)
            {
                playerref = new WeakReference<Player>(player);
            }
            public Scale[] tailScales = new Scale[6];
            public Vector2[] tailpositions = new Vector2[6];
            public Vector2[] headpositions = new Vector2[4];
            public Scale[] headScales = new Scale[4];
            public class Scale : BodyPart
            {
                public Scale(GraphicsModule cosmetics) : base(cosmetics)
                {

                }
                public override void Update()
                {
                    base.Update();
                    if (this.owner.owner.room.PointSubmerged(this.pos))
                    {
                        this.vel *= 0.5f;
                    }
                    else
                    {
                        this.vel *= 0.9f;
                    }
                    this.lastPos = this.pos;
                    this.pos += this.vel;
                }
                public float length = 10f;
                public float width = 0.7f;
            }
            public Color headcolor = new Color(1f, 1f, 0f);
            public Color tailcolor = new Color(1f, 1f, 0f);
            public int tailwhiskersprite(int side, int pair)
            {
                return initialtailwhiskerloc + side + pair + pair;
            }
            public int facewhiskersprite(int side, int pair)
            {
                return initialfacewhiskerloc + side + pair + pair;
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
                new OpKeyBinder(shockbind,new Vector2(10f,430f),new Vector2(30f,30f)),
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