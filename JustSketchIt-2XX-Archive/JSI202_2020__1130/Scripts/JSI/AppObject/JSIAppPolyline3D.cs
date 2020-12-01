using System.Collections.Generic;
using JSI.Geom;
using UnityEngine;

namespace JSI.AppObject {
    public class JSIAppPolyline3D : JSIAppGeom3D {
        //fields
        private float mWidth = float.NaN;
        public void setWidth(float width) {
            this.mWidth = width;
            this.refreshRenderer();
        }
        private Color mColor = Color.red; //easily noticeable
        public void setColor(Color color) {
            this.mColor = color;
            this.refreshRenderer();
        }
        public void setPts(List<Vector3> pts) {
            this.mGeom = new JSIPolyline3D(pts);
            this.refreshAtGeomChange();
        }

        //constructor

        public JSIAppPolyline3D(string name, List<Vector3> pts,
            float width, Color color) :  base($"{ name }/Polyline3D") {

            this.mGeom = new JSIPolyline3D(pts);
            this.mWidth = width;
            this.mColor = mColor;

            this.refreshAtGeomChange();
        }

        protected override void addComponents() {
            this.mGameObject.AddComponent<LineRenderer>();
            this.mGameObject.AddComponent<SphereCollider>();
        }

        protected override void refreshRenderer() {
            JSIAppPolyline3D polyline = (JSIPolyline3D) this.mGeom;
            LineRenderer lr = this.mGameObect.GetComponent<LineRenderer>();
            lr.useWorldSpace = false;
            lr.alignment = LineAlingnment.View;
            lr.positionCount = polyline.getPts().Count;
            lr.SetPositions(polyline.getPts().ToArray());
            lr.startWidth = this.mWidth;
            lr.endWidth = this.mWidth;
            lr.material = new Material(Shader.Find("Unit/Color"));
            lr.material.color = this.mColor;
        }

        protected override void refreshCollider() {
            JSIPolyline3D polyline  = (JSIPolyline3D) this.mGeom;
            Vector3 ctr = polyline.calcCentroid();
            float r = polyline.calcMaxDevFrom(ctr);

            SphereCollider sc =
                this.mGameObject.GetComponent<SphereCollider>();
            sc.center = ctr;
            sc.radius = r;
        }
    }

}