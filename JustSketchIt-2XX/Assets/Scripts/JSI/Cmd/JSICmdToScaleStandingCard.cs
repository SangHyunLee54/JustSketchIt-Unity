﻿using System.Text;
using X;
using UnityEngine;
using JSI.AppObject;
using System.Collections.Generic;
using JSI.Geom;
using JSI.Scenario;

namespace JSI.Cmd
{
    internal class JSICmdToScaleStandingCard : XLoggableCmd {
        
        // fields
        private Vector2 mPrevPt = Vector2.zero;
        private Vector2 mCurPt = Vector2.zero;

        private JSICmdToScaleStandingCard(XApp app) : base(app)
        {
            JSIApp jsi = (JSIApp)this.mApp;
            JSIPenMark pm = jsi.getPenMarkMgr().getLastPenMark();
            this.mPrevPt = pm.getRecentPt(1);
            this.mCurPt = pm.getRecentPt(0);
        }

        public static bool execute(XApp app)
        {
            JSICmdToScaleStandingCard cmd =
                new JSICmdToScaleStandingCard(app);
            return cmd.execute();
        }

        protected override bool defineCmd()
        {
            JSIApp app = (JSIApp)this.mApp;
            JSIPerspCameraPerson cp = app.getPerspCameraPerson();

            JSIEditStandingCardScenario scenario =
                JSIEditStandingCardScenario.getSingleton();
            JSIStandingCard standingCardToScale =
                scenario.getSelectedStandingCard();
            JSIAppRect3D card = standingCardToScale.getCard();
            JSIAppCircle3D stand = standingCardToScale.getStand();
            JSIAppCircle3D scaleHandle = standingCardToScale.getScaleHandle();

            List<JSIAppPolyline3D> ptCurve3Ds =
                standingCardToScale.getPtCurve3Ds();

            // create the card plane.
            Plane cardPlane = new Plane(
                standingCardToScale.getGameObject().transform.forward,
                standingCardToScale.getGameObject().transform.position);

            // project the previous screen point to the plane.
            Ray prevPtRay = cp.getCamera().ScreenPointToRay(this.mPrevPt);
            float prevPtDist = float.NaN;
            cardPlane.Raycast(prevPtRay, out prevPtDist);
            Vector3 prevPtOnPlane = prevPtRay.GetPoint(prevPtDist);

            // project the previous screen point to the plane.
            Ray curPtRay = cp.getCamera().ScreenPointToRay(this.mCurPt);
            float curPtDist = float.NaN;
            cardPlane.Raycast(curPtRay, out curPtDist);
            Vector3 curPtOnPlane = curPtRay.GetPoint(curPtDist);

            // calculate the scale factor.
            float scaleFactor = curPtOnPlane.y / prevPtOnPlane.y;

            // resize the card.
            JSIRect3D rect = (JSIRect3D)card.getGeom3D();
            float newCardWidth = scaleFactor * rect.getWidth();
            float newCardHeight = scaleFactor * rect.getHeight();
            card.setSize(newCardWidth, newCardHeight);

            // change the position of the standing card and its card.
            Vector3 standingCardPos =
                standingCardToScale.getGameObject().transform.position;
            Vector3 newStandingCardPos = 
                new Vector3(standingCardPos.x, standingCardPos.y * scaleFactor,
                    standingCardPos.z);
            
            standingCardToScale.getGameObject().transform.position =
                newStandingCardPos;

            // change the position of the stand
            Vector3 standLocalPos =
                stand.getGameObject().transform.localPosition;
            Vector3 newStandLocalPos = new Vector3(standLocalPos.x,
                scaleFactor * standLocalPos.y, standLocalPos.z);
            stand.getGameObject().transform.localPosition =
                newStandLocalPos;
            stand.setRadius(newCardWidth / 2.0f);

            // change the position of the scale handle            
            Vector3 scaleHandleLocalPos =
                scaleHandle.getGameObject().transform.localPosition;
            Vector3 newScaleHanleLocalPos = new Vector3(
                scaleHandleLocalPos.x, scaleFactor * scaleHandleLocalPos.y,
                scaleHandleLocalPos.z);
            scaleHandle.getGameObject().transform.localPosition =
                newScaleHanleLocalPos;

            // scale 3D points curves.
            if (ptCurve3Ds != null) {
                foreach (JSIAppPolyline3D ptCurve3D in ptCurve3Ds) {
                    JSIPolyline3D polyline =
                        (JSIPolyline3D)ptCurve3D.getGeom3D();
                    List<Vector3> scalePt3Ds = new List<Vector3>();
                    foreach (Vector3 pt3D in polyline.getPts()) {
                        Vector3 scaledPt3D = new Vector3(
                            pt3D.x * scaleFactor,
                            pt3D.y * scaleFactor,
                            pt3D.z);
                        scalePt3Ds.Add(scaledPt3D);
                    }
                    ptCurve3D.setPts(scalePt3Ds);
                }
            }

            return true;
        }

        protected override string createLog()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.GetType().Name).Append("\t");
            return sb.ToString();
        }
    }
}