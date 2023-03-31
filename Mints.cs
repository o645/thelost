using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevInterface;
using Fisobs.Core;
using Fisobs.Creatures;
using Fisobs.Properties;
using Fisobs.Sandbox;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;
using Mono;
using MonoMod;
using MonoMod.RuntimeDetour;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx.Logging;

#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
#pragma warning restore CS0618 // Type or member is obsolete
namespace thelost
{
    public static class CustomTemplates
    {
        public static CreatureTemplate.Type MintLizard = new(nameof(MintLizard), true);
       public static CreatureTemplate.Type Rotrat = new(nameof(Rotrat), true);
        public static void UnregisterValues()
        {
            if(MintLizard != null)
            {
                MintLizard.Unregister();
                MintLizard = null;
            }
            if (Rotrat != null)
            {
                Rotrat.Unregister();
                Rotrat = null;
            }
        }
    }
    public static class SandboxUnlockID
    {
        public static MultiplayerUnlocks.SandboxUnlockID MintLiz = new(nameof(MintLiz), true);
       public static MultiplayerUnlocks.SandboxUnlockID Rotrat = new(nameof(Rotrat), true);
        public static void UnregisterValues()
        {
            if(MintLiz != null)
            {
                MintLiz.Unregister();
                MintLiz = null;
            }
            if (Rotrat != null)
            {
                Rotrat.Unregister();
                Rotrat = null;
            }
        }
    }
    public class mintcritob : Critob
    {
        public mintcritob() : base(CustomTemplates.MintLizard)
        {
            Icon = new SimpleIcon("Kill_Black_Lizard", new Color(0.7254f, 1f, 0.9176f));

            LoadedPerformanceCost = 100f;
            SandboxPerformanceCost = new SandboxPerformanceCost(0.5f, 0.5f);
            RegisterUnlock(KillScore.Configurable(6), SandboxUnlockID.MintLiz);
            minthooks.ApplyMyHooks();
            
        }
        public override int ExpeditionScore()
        {
            return 6;
        }
        public override Color DevtoolsMapColor(AbstractCreature acrit)
        {
            return new Color(0.7254f, 1f, 0.9176f);
        }
        public override string DevtoolsMapName(AbstractCreature acrit)
        {
            return "MLiz";
        }
        public override IEnumerable<string> WorldFileAliases()
        {
            return new[] { "mintLizard", "mintliz", "mint" };
        }
        public override IEnumerable<RoomAttractivenessPanel.Category> DevtoolsRoomAttraction()
        {
            return new[] { RoomAttractivenessPanel.Category.Lizards, RoomAttractivenessPanel.Category.Swimming, RoomAttractivenessPanel.Category.LikesInside, RoomAttractivenessPanel.Category.LikesWater };
        }
        public override CreatureTemplate.Type ArenaFallback()
        {
            return CreatureTemplate.Type.BlackLizard;
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new LizardAI(acrit,acrit.world);
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new Lizard(acrit, acrit.world);
        }
        public override CreatureState CreateState(AbstractCreature acrit)
        {
            return new LizardState(acrit);
        }

        public override CreatureTemplate CreateTemplate()
        {
            return LizardBreeds.BreedTemplate(Type, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LizardTemplate), null, null, null);
        }

        public override void EstablishRelationships()
        {
            var s = new Relationships(Type);
            s.Rivals(CreatureTemplate.Type.LizardTemplate, .1f);
            s.HasDynamicRelationship(CreatureTemplate.Type.Slugcat, .5f);
            s.Fears(CreatureTemplate.Type.Vulture, .9f);
            s.Fears(CreatureTemplate.Type.KingVulture, 1f);
            s.Eats(CreatureTemplate.Type.TubeWorm, .025f);
            s.Eats(CreatureTemplate.Type.Scavenger, .8f);
            s.Eats(CreatureTemplate.Type.CicadaA, .05f);
            s.Eats(CreatureTemplate.Type.LanternMouse, .3f);
            s.Eats(CreatureTemplate.Type.BigSpider, .35f);
            s.Eats(CreatureTemplate.Type.EggBug, .45f);
            s.Eats(CreatureTemplate.Type.JetFish, .1f);
            s.Fears(CreatureTemplate.Type.BigEel, 1f);
            s.Eats(CreatureTemplate.Type.Centipede, .8f);
            s.Eats(CreatureTemplate.Type.BigNeedleWorm, .25f);
            s.Fears(CreatureTemplate.Type.DaddyLongLegs, 1f);
            s.Eats(CreatureTemplate.Type.SmallNeedleWorm, .3f);
            s.Eats(CreatureTemplate.Type.DropBug, .2f);
            s.Fears(CreatureTemplate.Type.RedCentipede, .9f);
            s.Fears(CreatureTemplate.Type.TentaclePlant, .2f);
            s.Eats(CreatureTemplate.Type.Hazer, .15f);
            s.FearedBy(CreatureTemplate.Type.LanternMouse, .7f);
            s.EatenBy(CreatureTemplate.Type.Vulture, .5f);
            s.FearedBy(CreatureTemplate.Type.CicadaA, .3f);
            s.FearedBy(CreatureTemplate.Type.JetFish, .2f);
            s.FearedBy(CreatureTemplate.Type.Slugcat, 1f);
            s.FearedBy(CreatureTemplate.Type.Scavenger, .5f);
            s.EatenBy(CreatureTemplate.Type.BigSpider, .3f);
            s.EatenBy(CreatureTemplate.Type.DaddyLongLegs, 1f);


        }
    }

    public static class minthooks
    {
        public static void ApplyMyHooks()
        {
            On.LizardLimb.ctor += limbnoises;
            On.Lizard.ctor += lizcolors;
            On.LizardAI.Update += spitterfix;
            On.LizardAI.ctor += spitter;
            On.LizardGraphics.Update += camoupdate;
            On.LizardAI.AggressiveBehavior += spitaggressive;
            On.LizardVoice.GetMyVoiceTrigger += mintvoice;
            On.LizardGraphics.DrawSprites += camofix;
            On.LizardGraphics.BodyColor += bodycolordynamic;
            On.LizardGraphics.ctor += mintcosmetics;
            On.LizardGraphics.DynamicBodyColor += mintcamo;
            On.LizardBreeds.BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate += mintbreed;
            Hook vishook = new Hook(typeof(Lizard).GetProperty("VisibilityBonus", BindingFlags.Public | BindingFlags.Instance).GetGetMethod(), typeof(minthooks).GetMethod("Lizard_VisibilityBonus_get", BindingFlags.Public | BindingFlags.Static));
            On.LizardGraphics.HeadColor += ihateselfstupidheadcolor;
            On.LizardCosmetics.TailFin.DrawSprites += camotailfin;
            On.LizardCosmetics.SpineSpikes.DrawSprites += camospines;
        }

        private static void camospines(On.LizardCosmetics.SpineSpikes.orig_DrawSprites orig, LizardCosmetics.SpineSpikes self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.lGraphics.lizard.Template.type == CustomTemplates.MintLizard)
            {
                for (int i = self.startSprite; i < self.startSprite + self.bumps; i++)
                {
                    float f = Mathf.Lerp(0.05f, self.spineLength / self.lGraphics.BodyAndTailLength, Mathf.InverseLerp((float)self.startSprite, (float)(self.startSprite + self.bumps - 1), (float)i));
                    sLeaser.sprites[i].color = self.lGraphics.BodyColor(f);
                }
            }
        }

        private static void camotailfin(On.LizardCosmetics.TailFin.orig_DrawSprites orig, LizardCosmetics.TailFin self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.lGraphics.lizard.Template.type == CustomTemplates.MintLizard)
            {
                for (int i = 0; i < 2; i++)
                {
                    int num = i * self.bumps;
                    for(int j = self.startSprite; j < self.startSprite+ self.bumps; j++)
                    {
                        float f = Mathf.Lerp(0.05f, self.spineLength / self.lGraphics.BodyAndTailLength, Mathf.InverseLerp((float)self.startSprite, (float)(self.startSprite + self.bumps - 1), (float)j));
                        sLeaser.sprites[j + num].color = self.lGraphics.BodyColor(f);
                    }
                }
            }
        }

        private static Color ihateselfstupidheadcolor(On.LizardGraphics.orig_HeadColor orig, LizardGraphics self, float timeStacker)
        {
            Color res = orig(self, timeStacker);
            if(self.lizard.Template.type == CustomTemplates.MintLizard) //because i couldn't get the stupid headcolor get hooks working we're doing self i guess!
            {
                if (self.whiteFlicker > 0 && (self.whiteFlicker > 15 || self.everySecondDraw))
                {
                    return new Color(1f, 1f, 1f);
                }
                float num = 1f - Mathf.Pow(0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(self.lastBlink, self.blink, timeStacker) * 2f * 3.1415927f), 1.5f + self.lizard.AI.excitement * 1.5f);
                if (self.headColorSetter != 0f)
                {
                    num = Mathf.Lerp(num, (self.headColorSetter > 0f) ? 1f : 0f, Mathf.Abs(self.headColorSetter));
                }
                if (self.flicker > 10)
                {
                    num = self.flickerColor;
                }
                num = Mathf.Lerp(num, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(self.lastVoiceVisualization, self.voiceVisualization, timeStacker)), 0.75f), Mathf.Lerp(self.lastVoiceVisualizationIntensity, self.voiceVisualizationIntensity, timeStacker));
                res = Color.Lerp(Color.Lerp(new Color(1f, 1f, 1f), self.whiteCamoColor, self.whiteCamoColorAmount),Color.Lerp(self.effectColor, self.whiteCamoColor, self.whiteCamoColorAmount), num);
            }
            return res;
        }

        public delegate float orig_VisibilityBonus(Lizard self);
        public static float Lizard_VisibilityBonus_get(orig_VisibilityBonus orig, Lizard self)
        {
            float res = orig(self);
            if(self.Template.type == CustomTemplates.MintLizard)
            {
                res = -(self.graphicsModule as LizardGraphics).Camouflaged;
            }
            return res;
        }
        private static void camoupdate(On.LizardGraphics.orig_Update orig, LizardGraphics self)
        {
            orig(self);
            if (self.lizard.Template.type == CustomTemplates.MintLizard)
            {
                if (self.lizard.dead)
                {
                    self.whiteCamoColorAmount = Mathf.Lerp(self.whiteCamoColorAmount, 0.3f, 0.01f);
                }
                else
                {
                    if ((self.lizard.State as LizardState).health < 0.6f && Random.value * 1.5f < (self.lizard.State as LizardState).health && Random.value < 1f / (self.lizard.Stunned ? 10f : 40f))
                    {
                        self.whiteGlitchFit = (int)Mathf.Lerp(5f, 40f, (1f - (self.lizard.State as LizardState).health) * Random.value);
                    }
                    if (self.whiteGlitchFit == 0 && self.lizard.Stunned && Random.value < 0.05f)
                    {
                        self.whiteGlitchFit = 2;
                    }
                    if (self.whiteGlitchFit > 0)
                    {
                        self.whiteGlitchFit--;
                        float f = 1f - (self.lizard.State as LizardState).health;
                        if (Random.value < 0.2f)
                        {
                            self.whiteCamoColorAmountDrag = 1f;
                        }
                        if (Random.value < 0.2f)
                        {
                            self.whiteCamoColorAmount = 1f;
                        }
                        if (Random.value < 0.5f)
                        {
                            self.whiteCamoColor = Color.Lerp(self.whiteCamoColor, new Color(Random.value, Random.value, Random.value), Mathf.Pow(f, 0.2f) * Mathf.Pow(Random.value, 0.1f));
                        }
                        if (Random.value < 0.33333334f)
                        {
                            self.whitePickUpColor = new Color(Random.value, Random.value, Random.value);
                        }
                    }
                    else if (self.showDominance > 0f)
                    {
                        self.whiteDominanceHue += Random.value * Mathf.Pow(self.showDominance, 2f) * 0.2f;
                        if (self.whiteDominanceHue > 1f)
                        {
                            self.whiteDominanceHue -= 1f;
                        }
                        self.whiteCamoColor = Color.Lerp(self.whiteCamoColor, Custom.HSL2RGB(self.whiteDominanceHue, 1f, 0.5f), Mathf.InverseLerp(0.5f, 1f, Mathf.Pow(self.showDominance, 0.5f)) * Random.value);
                        self.whiteCamoColorAmount = Mathf.Lerp(self.whiteCamoColorAmount, 1f - Mathf.Sin(Mathf.InverseLerp(0f, 1.1f, Mathf.Pow(self.showDominance, 0.5f)) * 3.1415927f), 0.1f);
                    }
                    else
                    {
                        if (self.lizard.animation == Lizard.Animation.ShootTongue || self.lizard.animation == Lizard.Animation.PrepareToLounge || self.lizard.animation == Lizard.Animation.Lounge)
                        {
                            self.whiteCamoColorAmountDrag = 0f;
                        }
                        else if (Random.value < 0.1f)
                        {
                            self.CamoAmountControlled();
                        }
                        self.whiteCamoColorAmount = Mathf.Clamp(Mathf.Lerp(self.whiteCamoColorAmount, self.whiteCamoColorAmountDrag, 0.1f * Random.value), 0.15f, 1f);
                        self.whiteCamoColor = Color.Lerp(self.whiteCamoColor, self.whitePickUpColor, 0.1f);
                    }
                }
            }
        }

        private static Color bodycolordynamic(On.LizardGraphics.orig_BodyColor orig, LizardGraphics self, float f)
        {
            Color res = orig(self, f);
            if(self.lizard.Template.type == CustomTemplates.MintLizard)
            {
                res = self.DynamicBodyColor(f);
            }
            return res;
        }

        private static void camofix(On.LizardGraphics.orig_DrawSprites orig, LizardGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if(self.lizard.Template.type == CustomTemplates.MintLizard)
            {
                self.ColorBody(sLeaser, self.DynamicBodyColor(0f));
                Color color = rCam.PixelColorAtCoordinate(self.lizard.mainBodyChunk.pos);
                Color color2 = rCam.PixelColorAtCoordinate(self.lizard.bodyChunks[1].pos);
                Color color3 = rCam.PixelColorAtCoordinate(self.lizard.bodyChunks[2].pos);
                if (color == color2)
                {
                    self.whitePickUpColor = color;
                }
                else if (color2 == color3)
                {
                    self.whitePickUpColor = color2;
                }
                else if (color3 == color)
                {
                    self.whitePickUpColor = color3;
                }
                else
                {
                    self.whitePickUpColor = (color + color2 + color3) / 3f;
                }
                if (self.whiteCamoColorAmount == -1f)
                {
                    self.whiteCamoColor = self.whitePickUpColor;
                    self.whiteCamoColorAmount = 1f;
                }
                int num7 = self.SpriteLimbsColorStart - self.SpriteLimbsStart;
                for (int m = self.SpriteLimbsStart; m < self.SpriteLimbsEnd; m++)
                    {
                        sLeaser.sprites[m + num7].alpha = Mathf.Sin(self.whiteCamoColorAmount * 3.1415927f) * 0.3f;
                        sLeaser.sprites[m + num7].color = self.effectColor;
                    }
            }
            orig(self,sLeaser,rCam,timeStacker,camPos);
        }

        private static Color mintcamo(On.LizardGraphics.orig_DynamicBodyColor orig, LizardGraphics self, float f)
        {
            Color res = orig(self, f);
            if(self.lizard.Template.type == CustomTemplates.MintLizard)
            {
                res = Color.Lerp(self.ivarBodyColor, self.whiteCamoColor, self.whiteCamoColorAmount);
            }
            return res;
        }

        private static void mintcosmetics(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            if(self.lizard.Template.type == CustomTemplates.MintLizard)
            {
                var state = Random.state;
                Random.InitState(self.lizard.abstractCreature.ID.RandomSeed);
                var num = self.startOfExtraSprites + self.extraSprites;
                self.ivarBodyColor = Color.white;
                num = self.AddCosmetic(num, new LizardCosmetics.Whiskers(self, num));
                if(Random.value < 0.4f)
                {
                    var e = new LizardCosmetics.LongHeadScales(self, num);
                    e.colored = false;
                    e.numberOfSprites = e.scalesPositions.Length;
                    num = self.AddCosmetic(num,e);
                    
                    
                }
                if(Random.value < 0.2f)
                {
                    var e = new LizardCosmetics.SpineSpikes(self, num);
                    e.colored = 0;
                    e.numberOfSprites = e.bumps;

                    num = self.AddCosmetic(num, e);
                    
                }
                if(Random.value < 0.8f)
                {
                    var e = new LizardCosmetics.TailFin(self, num);
                    e.colored = false;
                    e.numberOfSprites = e.bumps * 2;
                    num = self.AddCosmetic(num, e);
                }
                Random.state = state;
            }
        }

        private static void spitaggressive(On.LizardAI.orig_AggressiveBehavior orig, LizardAI self, Tracker.CreatureRepresentation target, float tongueChance)
        {
            orig(self,target, tongueChance);
            if (self.lizard.Template.type == CustomTemplates.MintLizard && target.VisualContact)
            {
                self.lizard.JawOpen = Mathf.Clamp(self.lizard.JawOpen + 0.1f, 0f, 1f);
            }
        }

        private static void spitterfix(On.LizardAI.orig_Update orig, LizardAI self)
        {
            orig(self);
            if (self.lizard.Template.type == CustomTemplates.MintLizard && self.redSpitAI.spitting)
            {
                self.lizard.EnterAnimation(Lizard.Animation.Spit, false);
            }
        }

        private static void spitter(On.LizardAI.orig_ctor orig, LizardAI self, AbstractCreature creature, World world)
        {
            orig(self,creature,world);
            
            if(self.lizard.Template.type == CustomTemplates.MintLizard)
            {
                self.AddModule(new SuperHearing(self, self.tracker, 250f));
                
                self.redSpitAI = new LizardAI.LizardSpitTracker(self);
                self.AddModule(self.redSpitAI);
            }
        }

        private static void lizcolors(On.Lizard.orig_ctor orig, Lizard self, AbstractCreature abstractCreature, World world)
        {
            orig(self,abstractCreature,world);
            if(self.Template.type == CustomTemplates.MintLizard)
            {
                var state = Random.state;
                Random.InitState(abstractCreature.ID.RandomSeed);
                self.effectColor = Custom.HSL2RGB(Custom.WrappedRandomVariation(0.45f, 0.05f, 0.5f), 1f, Custom.WrappedRandomVariation(0.86f, 0.05f, 0.5f));
                Random.state = state;
            }
        }

        private static CreatureTemplate mintbreed(On.LizardBreeds.orig_BreedTemplate_Type_CreatureTemplate_CreatureTemplate_CreatureTemplate_CreatureTemplate orig, CreatureTemplate.Type type, CreatureTemplate lizardAncestor, CreatureTemplate pinkTemplate, CreatureTemplate blueTemplate, CreatureTemplate greenTemplate)
        {
           if(type == CustomTemplates.MintLizard)
            {
                var temp = orig(CreatureTemplate.Type.BlackLizard, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
                var breedparams = (temp.breedParameters as LizardBreedParams);
                temp.type = type;
                temp.name = "Mint Lizard";
                breedparams.tailSegments = Random.Range(4, 10);
                breedparams.swimSpeed = 1f;
                breedparams.danger = 0.7f;
                breedparams.standardColor = new(0.7254f, 1f, 0.9176f);
                breedparams.headSize = 0.8f;
                temp.doPreBakedPathing = false;
                temp.preBakedPathingAncestor = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.PinkLizard);
                temp.requireAImap = true;
                breedparams.baseSpeed = 2f;
                breedparams.terrainSpeeds[1] = new(1f, 1f, 1f, 1f);
                breedparams.terrainSpeeds[2] = new(1f, 1f, 1f, 1f);
                breedparams.terrainSpeeds[3] = new(1f, 1f, 1f, 1f);
                temp.canSwim = true;
                breedparams.bodySizeFac = 0.8f;
                breedparams.bodyLengthFac = 1.4f;
                breedparams.bodyStiffnes = 0.8f;
                breedparams.shakePrey = 2;
                temp.dangerousToPlayer = breedparams.danger;
                temp.visualRadius = 20f;
                temp.waterVision = 1f;
                temp.waterPathingResistance = 1f;
                temp.throwAction = "Spit";
                
                return temp;
            }
            return orig(type, lizardAncestor, pinkTemplate, blueTemplate, greenTemplate);
        }

        private static SoundID mintvoice(On.LizardVoice.orig_GetMyVoiceTrigger orig, LizardVoice self)
        {
            var res = orig(self);
            if(self.lizard is Lizard l && l.Template.type == CustomTemplates.MintLizard)
            {
                string[] array = new[] { "Eel_A", "Eel_B","Black_A" };
                List<SoundID> list = new List<SoundID>();
                for (int i = 0; i < array.Length; i++)
                {
                    SoundID soundID = SoundID.None;
                    string text = "Lizard_Voice_" + array[i];
                    if (ExtEnum<SoundID>.values.entries.Contains(text))
                    {
                        soundID = new SoundID(text, false);
                    }
                    if (soundID != SoundID.None && soundID.Index != -1 && self.lizard.abstractCreature.world.game.soundLoader.workingTriggers[soundID.Index])
                    {
                        list.Add(soundID);
                    }
                }
                if (list.Count == 0)
                {
                    res = SoundID.None;
                }
                else
                {
                    res = list[Random.Range(0, list.Count)];
                }
                
            }

            return res;
        }

        private static void limbnoises(On.LizardLimb.orig_ctor orig, LizardLimb self, GraphicsModule owner, BodyChunk connectionChunk, int num, float rad, float sfFric, float aFric, float huntSpeed, float quickness, LizardLimb otherLimbInPair)
        {
            orig(self,owner,connectionChunk,num,rad,sfFric,aFric,huntSpeed, quickness, otherLimbInPair);
            if(owner is LizardGraphics l && l.lizard?.Template.type == CustomTemplates.MintLizard)
            {
                self.grabSound = SoundID.Lizard_BlueWhite_Foot_Grab;
                self.releaseSeound = SoundID.Lizard_BlueWhite_Foot_Release;
            }
        }
    }
}
