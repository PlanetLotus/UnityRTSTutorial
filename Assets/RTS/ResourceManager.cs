﻿using UnityEngine;
using System.Collections;

namespace RTS {
	public static class ResourceManager {
		public static float ScrollSpeed { get { return 25; } }
		public static float RotateSpeed { get { return 100; } }
        public static float RotateAmount { get { return 10; } }
		public static int ScrollWidth { get { return 15; } }
        public static float MinCameraHeight { get { return 10; } }
        public static float MaxCameraHeight { get { return 40; } }
        public static Vector3 InvalidPosition { get { return invalidPosition; } }
        public static GUISkin SelectBoxSkin { get { return selectBoxSkin; } }
        public static Bounds InvalidBounds { get { return invalidBounds; } }
        public static int BuildSpeed { get { return 2; } }

        public static void StoreSelectBoxItems(GUISkin skin) {
            selectBoxSkin = skin;
        }

        public static void SetGameObjectList(GameObjectList objectList) {
            gameObjectList = objectList;
        }

        public static GameObject GetBuilding(string name) {
            return gameObjectList.GetBuilding(name);
        }

        public static GameObject GetUnit(string name) {
            return gameObjectList.GetUnit(name);
        }

        public static GameObject GetWorldObject(string name) {
            return gameObjectList.GetWorldObject(name);
        }

        public static GameObject GetPlayerObject() {
            return gameObjectList.GetPlayerObject();
        }

        public static Texture2D GetBuildImage(string name) {
            return gameObjectList.GetBuildImage(name);
        }

        // Tutorial said that declaring a private variable and having a getter to it prevents it from being created multiple times...I don't think that's true.
        //  Shouldn't matter, so long as it's static. Confirm, out of curiosity?
        private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
        private static GUISkin selectBoxSkin;
        private static Bounds invalidBounds = new Bounds(invalidPosition, new Vector3(0, 0, 0));
        private static GameObjectList gameObjectList;
	}
}
