using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AfGD
{
    public struct Matrix3x3
    {
        public float m00;
        public float m10;
        public float m20;

        public float m01;
        public float m11;
        public float m12;

        public float m02;
        public float m21;
        public float m22;

        static readonly Matrix3x3 identityMatrix = new Matrix3x3(new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 1));

        public Matrix3x3(Vector3 column0, Vector3 column1, Vector3 column2)
        {
            this.m00 = column0.x; this.m01 = column1.x; this.m02 = column2.x;
            this.m10 = column0.y; this.m11 = column1.y; this.m12 = column2.y;
            this.m20 = column0.z; this.m21 = column1.z; this.m22 = column2.z;
        }



        public static Vector3 operator *(Matrix3x3 lhs, Vector3 vector)
        {
            Vector3 res;
            res.x = lhs.m00 * vector.x + lhs.m01 * vector.y + lhs.m02 * vector.z;
            res.y = lhs.m10 * vector.x + lhs.m11 * vector.y + lhs.m12 * vector.z;
            res.z = lhs.m20 * vector.x + lhs.m21 * vector.y + lhs.m22 * vector.z;
            return res;
        }
        public static Matrix3x3 operator *(Matrix3x3 lhs, Matrix3x3 rhs)
        {
            Matrix3x3 res;
            res.m00 = lhs.m00 * rhs.m00 + lhs.m01 * rhs.m10 + lhs.m02 * rhs.m20;
            res.m01 = lhs.m00 * rhs.m01 + lhs.m01 * rhs.m11 + lhs.m02 * rhs.m21;
            res.m02 = lhs.m00 * rhs.m02 + lhs.m01 * rhs.m12 + lhs.m02 * rhs.m22;

            res.m10 = lhs.m10 * rhs.m00 + lhs.m11 * rhs.m10 + lhs.m12 * rhs.m20;
            res.m11 = lhs.m10 * rhs.m01 + lhs.m11 * rhs.m11 + lhs.m12 * rhs.m21;
            res.m12 = lhs.m10 * rhs.m02 + lhs.m11 * rhs.m12 + lhs.m12 * rhs.m22;

            res.m20 = lhs.m20 * rhs.m00 + lhs.m21 * rhs.m10 + lhs.m22 * rhs.m20;
            res.m21 = lhs.m20 * rhs.m01 + lhs.m21 * rhs.m11 + lhs.m22 * rhs.m21;
            res.m22 = lhs.m20 * rhs.m02 + lhs.m21 * rhs.m12 + lhs.m22 * rhs.m22;

            return res;
        }

        public static Matrix3x3 Rotate(float angleInRadians)
        {
            Matrix3x3 res = identityMatrix;
            res.m00 = res.m11 = Mathf.Cos(angleInRadians);
            res.m10 = Mathf.Sin(angleInRadians);
            res.m01 = -res.m10;
            return res;
        }


    }

}