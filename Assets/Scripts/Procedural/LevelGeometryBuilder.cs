using UnityEngine;

namespace RunGame.Procedural
{
    public static class LevelGeometryBuilder
    {
        public static void CreateRoad(Transform parent, string name, float start, float end, Material material, bool colliderEnabled)
        {
            if (end <= start) return;
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = name;
            road.transform.SetParent(parent);
            road.transform.position = new Vector3(0f, -0.5f, (start + end) * 0.5f);
            road.transform.localScale = new Vector3(12f, 1f, end - start);
            road.GetComponent<Renderer>().sharedMaterial = material;
            if (!colliderEnabled) Object.Destroy(road.GetComponent<Collider>());
        }

        public static void CreatePlayerBoundary(Transform parent, float start, float end)
        {
            float length = end - start;
            CreateWall(parent, "Left Invisible Wall", new Vector3(-6.25f, 3.5f, (start + end) * 0.5f), new Vector3(0.3f, 8f, length));
            CreateWall(parent, "Right Invisible Wall", new Vector3(6.25f, 3.5f, (start + end) * 0.5f), new Vector3(0.3f, 8f, length));
            CreateWall(parent, "Start Invisible Wall", new Vector3(0f, 3.5f, start), new Vector3(12.8f, 8f, 0.3f));
            CreateWall(parent, "Finish Invisible Wall", new Vector3(0f, 3.5f, end), new Vector3(12.8f, 8f, 0.3f));
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().enabled = false;
            wall.AddComponent<PlayerBoundary>();
        }
    }
}
