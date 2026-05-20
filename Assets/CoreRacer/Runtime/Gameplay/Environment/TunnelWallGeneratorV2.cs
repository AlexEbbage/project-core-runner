using UnityEngine;

namespace CoreRacer.Gameplay.Environment
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class TunnelWallGeneratorV2 : MonoBehaviour
    {
        [SerializeField] private int sides = 6;
        [SerializeField] private float radius = 4f;
        [SerializeField] private float length = 240f;
        [SerializeField] private int lengthSegments = 48;
        [SerializeField] private bool invertNormals = true;

        private void Awake() => Rebuild();
        private void OnValidate() { if (Application.isPlaying) Rebuild(); }

        public void Configure(int tunnelSides, float tunnelRadius, float tunnelLength)
        {
            sides = Mathf.Max(3, tunnelSides);
            radius = Mathf.Max(0.5f, tunnelRadius);
            length = Mathf.Max(8f, tunnelLength);
            Rebuild();
        }

        [ContextMenu("Rebuild Tunnel")]
        public void Rebuild()
        {
            sides = Mathf.Max(3, sides);
            lengthSegments = Mathf.Max(1, lengthSegments);
            var mesh = new Mesh { name = "CoreRacer_TunnelWall" };
            var vertices = new Vector3[(lengthSegments + 1) * sides];
            var uvs = new Vector2[vertices.Length];
            var tris = new int[lengthSegments * sides * 6];

            for (int z = 0; z <= lengthSegments; z++)
            {
                var zPos = (z / (float)lengthSegments) * length;
                for (int s = 0; s < sides; s++)
                {
                    var angle = (s / (float)sides) * Mathf.PI * 2f;
                    var i = z * sides + s;
                    vertices[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, zPos);
                    uvs[i] = new Vector2(s / (float)sides, z / (float)lengthSegments);
                }
            }

            var ti = 0;
            for (int z = 0; z < lengthSegments; z++)
            {
                for (int s = 0; s < sides; s++)
                {
                    var a = z * sides + s;
                    var b = z * sides + (s + 1) % sides;
                    var c = (z + 1) * sides + s;
                    var d = (z + 1) * sides + (s + 1) % sides;
                    if (invertNormals)
                    {
                        tris[ti++] = a; tris[ti++] = c; tris[ti++] = b;
                        tris[ti++] = b; tris[ti++] = c; tris[ti++] = d;
                    }
                    else
                    {
                        tris[ti++] = a; tris[ti++] = b; tris[ti++] = c;
                        tris[ti++] = b; tris[ti++] = d; tris[ti++] = c;
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = tris;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
    }
}
