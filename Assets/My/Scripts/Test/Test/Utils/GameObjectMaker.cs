﻿using UnityEngine;

namespace Test.Utils
{
    public static class GameObjectMaker
    {
        public static GameObject CreateGameObject(string name, Mesh mesh)
        {
            var gameObject = new GameObject(name);
            var meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
            meshFilter.mesh = mesh;
            var renderer = (MeshRenderer)gameObject.AddComponent(typeof(MeshRenderer));
            renderer.material.shader = Shader.Find("Standard");
            return gameObject;
        }

        public static GameObject CreateGameObject(string name, Mesh mesh,
            Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            var gameObject = CreateGameObject(name, mesh);
            gameObject.transform.localPosition = translation;
            gameObject.transform.localEulerAngles = rotation;
            gameObject.transform.localScale = scale;
            return gameObject;
        }

        public static GameObject CreateGameObject(string name,
            Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.localPosition = translation;
            gameObject.transform.localEulerAngles = rotation;
            gameObject.transform.localScale = scale;
            return gameObject;
        }
    }
}
