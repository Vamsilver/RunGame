using UnityEngine;

namespace RunGame.Procedural
{
    public static class CityEnvironmentGenerator
    {
        public static void Generate(Transform parent, int seed, float start, float end)
        {
            GameObject[] buildingPrefabs = Resources.LoadAll<GameObject>("CityBuildings");
            if (buildingPrefabs.Length == 0) return;
            GameObject treePrefab = Resources.Load<GameObject>("CityProps/CityTree");
            GameObject carPrefab = Resources.Load<GameObject>("CityProps/CityCar");
            System.Random random = new(seed);
            float length = end - start;

            CreateSurface(parent, "City Pavement", new Vector3(-10f, -0.62f, (start + end) * 0.5f), new Vector3(8f, 0.24f, length), new Color(0.2f, 0.22f, 0.27f));
            CreateSurface(parent, "City Pavement", new Vector3(10f, -0.62f, (start + end) * 0.5f), new Vector3(8f, 0.24f, length), new Color(0.2f, 0.22f, 0.27f));
            CreateSurface(parent, "Grass Background", new Vector3(-22f, -0.68f, (start + end) * 0.5f), new Vector3(16f, 0.18f, length), new Color(0.08f, 0.48f, 0.16f));
            CreateSurface(parent, "Grass Background", new Vector3(22f, -0.68f, (start + end) * 0.5f), new Vector3(16f, 0.18f, length), new Color(0.08f, 0.48f, 0.16f));

            const float spacing = 7.2f;
            int slotCount = Mathf.CeilToInt(length / spacing);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int slot = 0; slot < slotCount; slot++)
                {
                    float z = start + 3.6f + slot * spacing;
                    if (z > end - 2f) break;
                    if (slot > 0 && slot % 5 == 0)
                    {
                        CreateIntersection(parent, carPrefab, random, side, z);
                        continue;
                    }

                    float width = 3.2f + (float)random.NextDouble() * 2.2f;
                    float depth = 3.4f + (float)random.NextDouble() * 2.4f;
                    float height = 4.5f + (float)random.NextDouble() * 7.5f;
                    float x = side * (9f + (float)random.NextDouble() * 3f);
                    GameObject building = Object.Instantiate(buildingPrefabs[random.Next(buildingPrefabs.Length)], parent);
                    building.name = $"City Building {side}-{slot + 1}";
                    building.transform.position = new Vector3(x, -0.5f, z);
                    building.transform.localScale = new Vector3(width / 4f, height / 6f, depth / 4f);

                    if (treePrefab == null) continue;
                    GameObject tree = Object.Instantiate(treePrefab, parent);
                    tree.name = "Background Tree";
                    tree.transform.position = new Vector3(side * (15f + (float)random.NextDouble() * 4f), -0.5f, z + ((float)random.NextDouble() - 0.5f) * 3f);
                    tree.transform.localScale = Vector3.one * (0.8f + (float)random.NextDouble() * 0.55f);
                }
            }
        }

        private static void CreateIntersection(Transform parent, GameObject carPrefab, System.Random random, int side, float z)
        {
            CreateSurface(parent, "Side Intersection", new Vector3(side * 17f, -0.54f, z), new Vector3(22f, 0.28f, 4.8f), new Color(0.075f, 0.085f, 0.105f));
            if (carPrefab == null) return;
            GameObject car = Object.Instantiate(carPrefab, parent);
            car.name = "Intersection Car";
            car.transform.position = new Vector3(side * (12f + (float)random.NextDouble() * 7f), -0.48f, z);
            car.transform.rotation = Quaternion.Euler(0f, side < 0 ? 90f : -90f, 0f);
        }

        private static void CreateSurface(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surface.name = name;
            surface.transform.SetParent(parent);
            surface.transform.position = position;
            surface.transform.localScale = scale;
            Object.Destroy(surface.GetComponent<Collider>());
            surface.GetComponent<Renderer>().material.color = color;
        }
    }
}
