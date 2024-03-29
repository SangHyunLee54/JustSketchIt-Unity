﻿using System.Text;
using X;
using UnityEngine;

namespace JSI.Scenario
{
    internal class JSICmdToDollyCamera : XLoggableCmd {

        private Vector2 mPrevPt = Vector2.zero;
        private Vector2 mCurPt = Vector2.zero;

        // private constructor
        private JSICmdToDollyCamera(XApp app) : base(app)
        {
            JSIApp jsi = (JSIApp)this.mApp;
            JSIPenMark pm = jsi.getPenMarkMgr().getLastPenMark();
            this.mPrevPt = pm.getRecentPt(1);
            this.mCurPt = pm.getRecentPt(0);
        }

        public static bool execute(XApp app)
        {
            JSICmdToDollyCamera cmd = new JSICmdToDollyCamera(app);
            return cmd.execute();
        }

        protected override bool defineCmd() {

            JSIApp app = (JSIApp) this.mApp;
            JSIPerspCameraPerson cp = app.getPerspCameraPerson();

            // create a plane on the pivot, directly facing the camera.
            Plane pivotPlane = new Plane(-cp.getView(), cp.getPivot());

            // project the previous screen point to the plane.
            Ray prevPtRay = cp.getCamera().ScreenPointToRay(this.mPrevPt);
            float prevPtDist = float.NaN;
            pivotPlane.Raycast(prevPtRay, out prevPtDist);
            Vector3 prevPtOnPlane = prevPtRay.GetPoint(prevPtDist);

            // project the current screen point to the plane.
            Ray curPtRay = cp.getCamera().ScreenPointToRay(this.mCurPt);
            float curPtDist = float.NaN;
            pivotPlane.Raycast(curPtRay, out curPtDist);
            Vector3 curPtOnPlane = curPtRay.GetPoint(curPtDist);

            // calculate the position difference between the two points.
            Vector3 offset = curPtOnPlane - prevPtOnPlane;

            // update the postion of the camera.
            cp.setEye(cp.getEye() - offset);

            return true;
        }   

        protected override string createLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetType().Name).Append("\t");
            sb.Append(this.mPrevPt).Append("\t");
            sb.Append(this.mCurPt);
            return sb.ToString();
        }
    }
}