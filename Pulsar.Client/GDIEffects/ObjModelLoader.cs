using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
namespace Pulsar.Client.GDIEffects
{
    /// <summary>
    /// Simple loader for OBJ model files
    /// </summary>
    public static class ObjModelLoader
    {
        /// <summary>
        /// Contains the model data extracted from an OBJ file
        /// </summary>
        public class ModelData
        {
            /// <summary>
            /// Processed vertices with position, texcoord, and normal data
            /// </summary>
            public Vector3[] Positions { get; set; }

            /// <summary>
            /// Texture coordinates
            /// </summary>
            public Vector2[] TexCoords { get; set; }

            /// <summary>
            /// Normal vectors
            /// </summary>
            public Vector3[] Normals { get; set; }

            /// <summary>
            /// Indices for the triangles
            /// </summary>
            public int[] Indices { get; set; }

            /// <summary>
            /// Path to the texture file
            /// </summary>
            public string TexturePath { get; set; }
        }

        /// <summary>
        /// Represents a vertex with position, texture, and normal indices
        /// </summary>
        private struct VertexDefinition
        {
            public int PositionIndex;
            public int TexCoordIndex;
            public int NormalIndex;

            public override bool Equals(object obj)
            {
                if (!(obj is VertexDefinition))
                    return false;

                VertexDefinition other = (VertexDefinition)obj;
                return PositionIndex == other.PositionIndex && 
                       TexCoordIndex == other.TexCoordIndex && 
                       NormalIndex == other.NormalIndex;
            }

            public override int GetHashCode()
            {
                return PositionIndex.GetHashCode() ^ 
                       TexCoordIndex.GetHashCode() ^ 
                       NormalIndex.GetHashCode();
            }
        }
        /// <summary>
        /// Loads an OBJ file and returns the model data
        /// </summary>
        public static ModelData LoadObj(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"obj file {filePath} wasnt found");
            }

            Debug.WriteLine($"Loading obj {filePath}");


            List<Vector3> positions = new List<Vector3>();
            List<Vector2> texCoords = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();


            Dictionary<VertexDefinition, int> uniqueVertices = new Dictionary<VertexDefinition, int>();


            List<Vector3> finalPositions = new List<Vector3>();
            List<Vector2> finalTexCoords = new List<Vector2>();
            List<Vector3> finalNormals = new List<Vector3>();

            string texturePath = null;
            string mtlPath = null;

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length == 0)
                        continue;

                    string keyword = parts[0].ToLowerInvariant();

                    switch (keyword)
                    {
                        case "v": 
                            if (parts.Length >= 4)
                            {
                                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                                positions.Add(new Vector3(x, y, z));
                            }
                            break;

                        case "vt": 
                            if (parts.Length >= 3)
                            {
                                float u = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                float v = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                texCoords.Add(new Vector2(u, 1.0f - v)); 
                            }
                            break;

                        case "vn": 
                            if (parts.Length >= 4)
                            {
                                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                                normals.Add(new Vector3(x, y, z));
                            }
                            break;

                        case "f": 


                            if (parts.Length >= 4)
                            {
                                for (int i = 1; i <= 3; i++) 
                                {
                                    string[] vertexParts = parts[i].Split('/');

                                    VertexDefinition vertDef = new VertexDefinition();


                                    vertDef.PositionIndex = int.Parse(vertexParts[0]) - 1;


                                    vertDef.TexCoordIndex = vertexParts.Length > 1 && !string.IsNullOrEmpty(vertexParts[1]) 
                                        ? int.Parse(vertexParts[1]) - 1 
                                        : -1;


                                    vertDef.NormalIndex = vertexParts.Length > 2 && !string.IsNullOrEmpty(vertexParts[2]) 
                                        ? int.Parse(vertexParts[2]) - 1 
                                        : -1;


                                    if (!uniqueVertices.TryGetValue(vertDef, out int index))
                                    {

                                        index = finalPositions.Count;
                                        uniqueVertices[vertDef] = index;

                                        finalPositions.Add(positions[vertDef.PositionIndex]);

                                        if (vertDef.TexCoordIndex >= 0 && vertDef.TexCoordIndex < texCoords.Count)
                                            finalTexCoords.Add(texCoords[vertDef.TexCoordIndex]);
                                        else
                                            finalTexCoords.Add(new Vector2(0, 0));

                                        if (vertDef.NormalIndex >= 0 && vertDef.NormalIndex < normals.Count)
                                            finalNormals.Add(normals[vertDef.NormalIndex]);
                                        else
                                            finalNormals.Add(Vector3.UnitY);
                                    }


                                    indices.Add(index);
                                }


                                if (parts.Length > 4)
                                {
                                    for (int i = 3; i < parts.Length - 1; i++)
                                    {

                                        string[] vertex1Parts = parts[1].Split('/');
                                        string[] vertexIParts = parts[i].Split('/');
                                        string[] vertexIPlus1Parts = parts[i + 1].Split('/');



                                        VertexDefinition vertDef1 = new VertexDefinition();
                                        vertDef1.PositionIndex = int.Parse(vertex1Parts[0]) - 1;
                                        vertDef1.TexCoordIndex = vertex1Parts.Length > 1 && !string.IsNullOrEmpty(vertex1Parts[1]) 
                                            ? int.Parse(vertex1Parts[1]) - 1 
                                            : -1;
                                        vertDef1.NormalIndex = vertex1Parts.Length > 2 && !string.IsNullOrEmpty(vertex1Parts[2]) 
                                            ? int.Parse(vertex1Parts[2]) - 1 
                                            : -1;

                                        if (!uniqueVertices.TryGetValue(vertDef1, out int index1))
                                        {
                                            index1 = finalPositions.Count;
                                            uniqueVertices[vertDef1] = index1;

                                            finalPositions.Add(positions[vertDef1.PositionIndex]);

                                            if (vertDef1.TexCoordIndex >= 0 && vertDef1.TexCoordIndex < texCoords.Count)
                                                finalTexCoords.Add(texCoords[vertDef1.TexCoordIndex]);
                                            else
                                                finalTexCoords.Add(new Vector2(0, 0));

                                            if (vertDef1.NormalIndex >= 0 && vertDef1.NormalIndex < normals.Count)
                                                finalNormals.Add(normals[vertDef1.NormalIndex]);
                                            else
                                                finalNormals.Add(Vector3.UnitY);
                                        }


                                        VertexDefinition vertDefI = new VertexDefinition();
                                        vertDefI.PositionIndex = int.Parse(vertexIParts[0]) - 1;
                                        vertDefI.TexCoordIndex = vertexIParts.Length > 1 && !string.IsNullOrEmpty(vertexIParts[1]) 
                                            ? int.Parse(vertexIParts[1]) - 1 
                                            : -1;
                                        vertDefI.NormalIndex = vertexIParts.Length > 2 && !string.IsNullOrEmpty(vertexIParts[2]) 
                                            ? int.Parse(vertexIParts[2]) - 1 
                                            : -1;

                                        if (!uniqueVertices.TryGetValue(vertDefI, out int indexI))
                                        {
                                            indexI = finalPositions.Count;
                                            uniqueVertices[vertDefI] = indexI;

                                            finalPositions.Add(positions[vertDefI.PositionIndex]);

                                            if (vertDefI.TexCoordIndex >= 0 && vertDefI.TexCoordIndex < texCoords.Count)
                                                finalTexCoords.Add(texCoords[vertDefI.TexCoordIndex]);
                                            else
                                                finalTexCoords.Add(new Vector2(0, 0));

                                            if (vertDefI.NormalIndex >= 0 && vertDefI.NormalIndex < normals.Count)
                                                finalNormals.Add(normals[vertDefI.NormalIndex]);
                                            else
                                                finalNormals.Add(Vector3.UnitY);
                                        }


                                        VertexDefinition vertDefIPlus1 = new VertexDefinition();
                                        vertDefIPlus1.PositionIndex = int.Parse(vertexIPlus1Parts[0]) - 1;
                                        vertDefIPlus1.TexCoordIndex = vertexIPlus1Parts.Length > 1 && !string.IsNullOrEmpty(vertexIPlus1Parts[1]) 
                                            ? int.Parse(vertexIPlus1Parts[1]) - 1 
                                            : -1;
                                        vertDefIPlus1.NormalIndex = vertexIPlus1Parts.Length > 2 && !string.IsNullOrEmpty(vertexIPlus1Parts[2]) 
                                            ? int.Parse(vertexIPlus1Parts[2]) - 1 
                                            : -1;

                                        if (!uniqueVertices.TryGetValue(vertDefIPlus1, out int indexIPlus1))
                                        {
                                            indexIPlus1 = finalPositions.Count;
                                            uniqueVertices[vertDefIPlus1] = indexIPlus1;

                                            finalPositions.Add(positions[vertDefIPlus1.PositionIndex]);

                                            if (vertDefIPlus1.TexCoordIndex >= 0 && vertDefIPlus1.TexCoordIndex < texCoords.Count)
                                                finalTexCoords.Add(texCoords[vertDefIPlus1.TexCoordIndex]);
                                            else
                                                finalTexCoords.Add(new Vector2(0, 0));

                                            if (vertDefIPlus1.NormalIndex >= 0 && vertDefIPlus1.NormalIndex < normals.Count)
                                                finalNormals.Add(normals[vertDefIPlus1.NormalIndex]);
                                            else
                                                finalNormals.Add(Vector3.UnitY);
                                        }


                                        indices.Add(index1);
                                        indices.Add(indexI);
                                        indices.Add(indexIPlus1);
                                    }
                                }
                            }
                            break;

                        case "mtllib": 
                            if (parts.Length >= 2)
                            {
                                mtlPath = Path.Combine(Path.GetDirectoryName(filePath), parts[1]);
                            }
                            break;
                    }
                }
            }


            if (!string.IsNullOrEmpty(mtlPath) && File.Exists(mtlPath))
            {

                using (StreamReader reader = new StreamReader(mtlPath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                            continue;

                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length == 0)
                            continue;

                        string keyword = parts[0].ToLowerInvariant();


                        if (keyword == "map_kd" && parts.Length >= 2)
                        {
                            texturePath = Path.Combine(Path.GetDirectoryName(mtlPath), parts[parts.Length - 1]);
                            break;
                        }
                    }
                }
            }


            var modelData = new ModelData
            {
                Positions = finalPositions.ToArray(),
                TexCoords = finalTexCoords.ToArray(),
                Normals = finalNormals.ToArray(),
                Indices = indices.ToArray(),
                TexturePath = texturePath
            };

            Debug.WriteLine($"Loaded obj model with {finalPositions.Count} vertices, {indices.Count} indices, {(texturePath != null ? "with texture" : "no texture")}");

            return modelData;
        }
    }
} 
