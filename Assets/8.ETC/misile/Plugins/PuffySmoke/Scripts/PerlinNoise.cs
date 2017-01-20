using UnityEngine;

/*
 * 
 * Perlin noise class.  ( by Tom Nuydens (tom@delphi3d.net) )
 * Converted to C# by Mattias Fagerlund, Mattias.Fagerlund@cortego.se
 * 
 * source : https://lotsacode.wordpress.com/2010/02/24/perlin-noise-in-c/
 * 
 * Note : Tweaked a bit for my needs
 * 
 */

public class PerlinNoise{

	private const int GradientSizeTable = 256;
    private readonly float[] _gradients = new float[GradientSizeTable * 3];
    private readonly byte[] _perm = new byte[GradientSizeTable*2];

    public PerlinNoise(int seed=0)
    {
		Seed (seed);
    }
	
	public void Seed(int seed){
		int prevSeed = UnityEngine.Random.seed;
		UnityEngine.Random.seed = seed;
		
		int i, j, k;
		for (i = 0 ; i < GradientSizeTable ; i++) 
		{
			_perm[i] = (byte)i;
		}

		while (--i != 0) 
		{
			k = _perm[i];
			j = UnityEngine.Random.Range(0, GradientSizeTable);
			_perm[i] = _perm[j];
			_perm[j] = (byte)k;
		}
	
		for (i = 0 ; i < GradientSizeTable; i++) 
		{
			_perm[GradientSizeTable + i] = _perm[i];
		}
		
		InitGradients();
		
		UnityEngine.Random.seed = prevSeed;
	}
	
    public float Noise(Vector3 position)
    {
        /* The main noise function. Looks up the pseudorandom gradients at the nearest
           lattice points, dots them with the input vector, and interpolates the
           results to produce a single output value in [0, 1] range. */

		int ix = (int)Mathf.Floor(position.x);
		float fx0 = position.x - ix;
        float fx1 = fx0 - 1;
        float wx = Smooth(fx0);

		int iy = (int)Mathf.Floor(position.y);
		float fy0 = position.y - iy;
        float fy1 = fy0 - 1;
        float wy = Smooth(fy0);

		int iz = (int)Mathf.Floor(position.z);
		float fz0 = position.z - iz;
        float fz1 = fz0 - 1;
        float wz = Smooth(fz0);

        float vx0 = Lattice(ix, iy, iz, fx0, fy0, fz0);
        float vx1 = Lattice(ix + 1, iy, iz, fx1, fy0, fz0);
        float vy0 = Mathf.Lerp(vx0, vx1,wx);

        vx0 = Lattice(ix, iy + 1, iz, fx0, fy1, fz0);
        vx1 = Lattice(ix + 1, iy + 1, iz, fx1, fy1, fz0);
        float vy1 = Mathf.Lerp(vx0, vx1,wx);

        float vz0 = Mathf.Lerp(vy0, vy1,wy);

        vx0 = Lattice(ix, iy, iz + 1, fx0, fy0, fz1);
        vx1 = Lattice(ix + 1, iy, iz + 1, fx1, fy0, fz1);
        vy0 = Mathf.Lerp(vx0, vx1,wx);

        vx0 = Lattice(ix, iy + 1, iz + 1, fx0, fy1, fz1);
        vx1 = Lattice(ix + 1, iy + 1, iz + 1, fx1, fy1, fz1);
        vy1 = Mathf.Lerp(vx0, vx1,wx);
		
		return Mathf.Lerp(vz0, Mathf.Lerp(vy0, vy1,wy) ,wz);
    }

	public float FractalNoise(Vector3 position, int octNum, float frq, float amp)
	{
		float gain = 1.0f;
		float sum = 0.0f;
		
		for(int i = 0; i < octNum; i++)
		{
			Vector3 pos = position * gain / frq;
			sum +=  Noise(pos) * amp/gain;
			gain *= 2.0f;
		}
		return sum;
	}

	
    private void InitGradients()
    {
        for (int i = 0; i < GradientSizeTable; i++)
        {
            float z = 1f - 2f * Random.value;
            float r = Mathf.Sqrt(1f - z * z);
            float theta = 2 * Mathf.PI * Random.value;
            _gradients[i * 3] = r * Mathf.Cos(theta);
            _gradients[i * 3 + 1] = r * Mathf.Sin(theta);
            _gradients[i * 3 + 2] = z;
        }
    }

    private int Permutate(int x)
    {
        const int mask = GradientSizeTable - 1;
        return _perm[x & mask];
    }

    private int Index(int ix, int iy, int iz)
    {
        // Turn an XYZ triplet into a single gradient table index.
        return Permutate(ix + Permutate(iy + Permutate(iz)));
    }

    private float Lattice(int ix, int iy, int iz, float fx, float fy, float fz)
    {
        // Look up a random gradient at [ix,iy,iz] and dot it with the [fx,fy,fz] vector.
        int index = Index(ix, iy, iz);
        int g = index*3;
        return _gradients[g] * fx + _gradients[g + 1] * fy + _gradients[g + 2] * fz;
    }

    private float Smooth(float x)
    {
        /* Smoothing curve. This is used to calculate interpolants so that the noise
          doesn't look blocky when the frequency is low. */
        return x * x * (3 - 2 * x);
    }

}
