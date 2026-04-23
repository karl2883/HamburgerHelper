using Celeste.Mod.Backdrops;

namespace Celeste.Mod.HamburgerHelper.Backdrops;

[CustomBackdrop("HamburgerHelper/Fireworks")]
public class Fireworks : Backdrop
{
    private struct Particle
    {
        public Vector2 Position;
        public Vector2 Speed;
        public Vector2 Velocity;
        public Color Color;
        public float Life;
        public float Fade;
    }
    
    private readonly Particle[] Particles = new Particle[640];
    private int NextAvailableParticle = 0;
    
    public Fireworks(BinaryPacker.Element data)
    {
    }

    public override void Update(Scene scene)
    {
        base.Update(scene);
        
        for (int i = 0; i < Particles.Length; i++)
        {
            if (Particles[i].Fade <= 0) continue;

            Particles[i].Speed += Particles[i].Velocity;
            Particles[i].Position += Particles[i].Speed;
            Particles[i].Life += Engine.DeltaTime;

            if (Particles[i].Life > 0.25)
            {
                Particles[i].Fade -= Engine.DeltaTime;   
            }
        }
    }

    public override void Render(Scene scene)
    {
        base.Render(scene);
        
        foreach (Particle particle in Particles)
        {
            if (particle.Fade <= 0) continue;
            
            Draw.Point(particle.Position, particle.Color * particle.Fade);
        }
    }

    private void CreateParticle(Vector2 position, Vector2 speed, Vector2 velocity, Color color)
    {
        Particle particle = new Particle 
        {
            Life = 0f,
            Fade = 0.5f,
            Position = position,
            Velocity = velocity,
            Speed = speed,
            Color = color
        };
        
        Particles[NextAvailableParticle % Particles.Length] = particle;
        NextAvailableParticle++;
    }
    
    public void CreateFirework(List<Color> colors)
    {
        const int ringSize = 32;
        const int ringCount = 5;

        float fireworkX = Calc.Random.NextFloat(322);
        float fireworkY = Calc.Random.NextFloat(182);
        Vector2 fireworkPos = new Vector2(fireworkX, fireworkY);
        
        for (int ring = 0; ring < ringCount; ring++)
        {
            float particleSpeed = 0.5f + (ring * 0.0625f);
            float angleOffset = Calc.Random.NextFloat(4);
            Color color = Calc.Random.Choose(colors);
            
            
            for (int i = 0; i < ringSize; i++)
            {
                float angle = (MathHelper.TwoPi * i / ringSize) + angleOffset;
                Vector2 speed = Calc.AngleToVector(angle, particleSpeed);
                Vector2 velocity = speed * 0.125f;

                CreateParticle(fireworkPos, speed, velocity, color);
            }
        }
    }
}
