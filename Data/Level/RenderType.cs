using System;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Data
{
    public abstract class RenderType
    {
        public abstract RenderTypeID Identifier { get; }
    }

    public class PolymodelRenderType : RenderType
    {
        public override RenderTypeID Identifier => RenderTypeID.Polyobj;

        public int ModelNum { get; set; }
        public FixAngles[] BodyAngles { get; } = new FixAngles[Polymodel.MAX_SUBMODELS];
        /// <summary>
        /// Specifies which subobjects to render. Set to 0 to make all of the model's submodels render.
        /// </summary>
        public int Flags { get; set; }
        public int TextureOverride { get; set; }
    }

    public class MorphRenderType : PolymodelRenderType
    {
        public override RenderTypeID Identifier => RenderTypeID.Morph;
    }

    public class FireballRenderType : RenderType
    {
        public override RenderTypeID Identifier => RenderTypeID.Fireball;

        public int VClipNum { get; set; }
        public Fix FrameTime { get; set; }
        public byte FrameNumber { get; set; }
    }

    //This is annoying: Hostages and Weapon VClips use the same storage data, but are drawn differently
    public class HostageRenderType : FireballRenderType
    {
        public override RenderTypeID Identifier => RenderTypeID.Hostage;
    }

    public class WeaponVClipRenderType : FireballRenderType
    {
        public override RenderTypeID Identifier => RenderTypeID.WeaponVClip;
    }
}
