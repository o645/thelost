using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace thelost
{
    internal class WhiskerStuff
    {
        public static void Hooks()
        {
            On.PlayerGraphics.ctor += Whiskersetup;
            On.PlayerGraphics.InitiateSprites += Funnytailwhiskerinit;
            On.PlayerGraphics.AddToContainer += Whiskercontainer;
            On.PlayerGraphics.DrawSprites += WhiskerDraw;
            On.PlayerGraphics.Update += Whiskerupdate;
        }
        public static ConditionalWeakTable<Player, Whiskerdata> tailwhiskerstorage = new ConditionalWeakTable<Player, Whiskerdata>();
        public class Whiskerdata
        {
            public bool ready = false;
            public int initialtailwhiskerloc; //initial location for each sprite!!
            public int initialfacewhiskerloc;
            public string sprite = "LizardScaleA0"; //just for changing out what sprite is used
            public string facesprite = "LizardScaleA0";
            public WeakReference<Player> playerref;
            public Whiskerdata(Player player) //sets up a weak reference to the player.
            {
                playerref = new WeakReference<Player>(player);
            }
            public Scale[] tailScales = new Scale[6]; //each scale
            public Vector2[] tailpositions = new Vector2[6]; //and their positions
            public Vector2[] headpositions = new Vector2[4]; // since lost has tail and head whiskers
            public Scale[] headScales = new Scale[4]; // theres two pairs of scale + position arrays!
            //scales are literaly stolen from rivulet's gill scales :]
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
            public Color headcolor = new Color(1f, 1f, 0f); //the color!
            public Color tailcolor = new Color(1f, 1f, 0f);
            public int tailwhiskersprite(int side, int pair) //i have no idea.
            {
                return initialtailwhiskerloc + side + pair + pair;
            }
            public int facewhiskersprite(int side, int pair)
            {
                return initialfacewhiskerloc + side + pair + pair;
            }
        }
        public static void Whiskersetup(On.PlayerGraphics.orig_ctor orig, PlayerGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if ((self.player).slugcatStats.name.value == "thelost")
            {
                tailwhiskerstorage.Add(self.player, new Whiskerdata(self.player)); //setup the CWT
                tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data); // really im stupid this could just have been setup before adding it to the cwt!
                for (int i = 0; i < data.tailScales.Length; i++) //some loops for setting up the arrays
                {
                    data.tailScales[i] = new Whiskerdata.Scale(self);
                    data.tailpositions[i] = new Vector2((i < data.tailScales.Length / 2 ? 0.7f : -0.7f), 0.28f);

                }
                for (int i = 0; i < data.headScales.Length; i++)
                {
                    data.headScales[i] = new Whiskerdata.Scale(self);
                    data.headpositions[i] = new Vector2((i < data.headScales.Length / 2 ? 0.7f : -0.7f), i == 1 ? 0.035f : 0.026f);

                }


            }
        }

        public static void Funnytailwhiskerinit(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if ((self.owner as Player).slugcatStats.name.value == "thelost")
            {

                tailwhiskerstorage.TryGetValue(self.player, out var thedata); //get out the data from thw CWT
                thedata.initialtailwhiskerloc = sLeaser.sprites.Length; //set the initial location to the current length of the sleaser.
                thedata.initialfacewhiskerloc = sLeaser.sprites.Length + 6; //add on 6 more bc theres 6 tail sprites
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + 6 + 4); //add on more space for our sprites
                // 6 for tail sprites, 4 for face sprites.
                for (int i = 0; i < 2; i++) //HELL LOOPS BEGIN!!!
                {
                    for (int j = 0; j < 3; j++)
                    {
                        sLeaser.sprites[thedata.tailwhiskersprite(i, j)] = new FSprite(thedata.sprite); //get which sprite we want from teh cwt
                        sLeaser.sprites[thedata.tailwhiskersprite(i, j)].scaleY = 10f / Futile.atlasManager.GetElementWithName(thedata.sprite).sourcePixelSize.y; //uh. no idea lol
                        sLeaser.sprites[thedata.tailwhiskersprite(i, j)].anchorY = 0.1f; //same here. iirc i just yoinked code from rivvy
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)] = new FSprite(thedata.facesprite);

                        sLeaser.sprites[thedata.facewhiskersprite(i, j)].scaleY = 10f / Futile.atlasManager.GetElementWithName(thedata.sprite).sourcePixelSize.y;
                        sLeaser.sprites[thedata.facewhiskersprite(i, j)].anchorY = 0.1f;
                    }
                }
                thedata.ready = true; //say that we're ready to add these to the container!
                self.AddToContainer(sLeaser, rCam, null); //then add em!
            }
        }

        public static void Whiskercontainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if ((self.player).slugcatStats.name.value == "thelost" && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data) && data.ready) //make sure to check that we're ready
            {
                FContainer container = rCam.ReturnFContainer("Midground"); //get the midground container
                for (int i = 0; i < 2; i++) //HELL LOOPS 2!!!
                {
                    for (int j = 0; j < 3; j++)
                    {
                        FSprite whisker = sLeaser.sprites[data.tailwhiskersprite(i, j)]; //get that fsprite
                        container.AddChild(whisker); //add it to container
                        if (i == 0)
                        {
                            whisker.MoveBehindOtherNode(sLeaser.sprites[2]); //bc these are tail sprites, half of em are gonna be behind the tail
                            whisker.MoveToBack(); //just incase. probably pointless but idk i did it anyway

                        }
                        else
                        {
                            whisker.MoveInFrontOfOtherNode(sLeaser.sprites[2]); //move infront!!! probably pointless too.

                        }

                    }
                }
                for (int i = 0; i < 2; i++) //same thing as before but for the head sprites
                {
                    for (int j = 0; j < 2; j++)
                    {
                        FSprite whisker = sLeaser.sprites[data.facewhiskersprite(i, j)];
                        container.AddChild(whisker);
                    }
                }
                data.ready = false; //set ready to false for next time.
            }


        }
        public static void WhiskerDraw(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {

            orig(self, sLeaser, rCam, timeStacker, camPos);
            if ((self.player).slugcatStats.name.value == "thelost" && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data))
            {
                //oh god i need to rewrite all of this into one for loop. <-- past me is right, you probably want to do this better. its a mess
                int index = 0;
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    { // this is all just yoinked from rivulet's code tbh. minor adjustments to make it work for the tail
                        Vector2 vector = new Vector2((sLeaser.sprites[2] as TriangleMesh).vertices[10].x + camPos.x, (sLeaser.sprites[2] as TriangleMesh).vertices[10].y + camPos.y);
                        Vector2 lol = new Vector2((sLeaser.sprites[2] as TriangleMesh).vertices[12].x + camPos.x, (sLeaser.sprites[2] as TriangleMesh).vertices[12].y + camPos.y);
                        float f = 0f;
                        float num = Custom.VecToDeg(Vector2.Lerp((sLeaser.sprites[2] as TriangleMesh).vertices[10], (sLeaser.sprites[2] as TriangleMesh).vertices[12], timeStacker));
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
                for (int i = 0; i < 2; i++) //as i said before, basically just rivy's code.
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



        public static void Whiskerupdate(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            if ((self.player).slugcatStats.name.value == "thelost" && tailwhiskerstorage.TryGetValue(self.player, out Whiskerdata data))
            {
                int index = 0; // once again we are in horrid loop hell. ew.
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Vector2 pos = self.tail[2].pos; //this is once again stolen from riv's code. just modified for tail reasons
                        Vector2 pos2 = self.tail[3].pos;
                        float num = 0f;
                        float num2 = 90f;
                        int num3 = index % (data.tailScales.Length / 2);
                        float num4 = num2 / (float)(data.tailScales.Length / 2);
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
                        Vector2 pos = self.owner.bodyChunks[0].pos; //the lost got a whisker transplant from rivvy... now rivvy has no whiskers so sad....
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



    }
}
