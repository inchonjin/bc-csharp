﻿using System;

namespace Org.BouncyCastle.Math.EC.Custom.Sec
{
    internal class SecP192K1Point
        : ECPointBase
    {
        /**
         * Create a point which encodes with point compression.
         * 
         * @param curve
         *            the curve to use
         * @param x
         *            affine x co-ordinate
         * @param y
         *            affine y co-ordinate
         * 
         * @deprecated Use ECCurve.createPoint to construct points
         */
        public SecP192K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y)
            : this(curve, x, y, false)
        {
        }

        /**
         * Create a point that encodes with or without point compresion.
         * 
         * @param curve
         *            the curve to use
         * @param x
         *            affine x co-ordinate
         * @param y
         *            affine y co-ordinate
         * @param withCompression
         *            if true encode with point compression
         * 
         * @deprecated per-point compression property will be removed, refer
         *             {@link #getEncoded(bool)}
         */
        public SecP192K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, bool withCompression)
            : base(curve, x, y, withCompression)
        {
            if ((x == null) != (y == null))
                throw new ArgumentException("Exactly one of the field elements is null");
        }

        internal SecP192K1Point(ECCurve curve, ECFieldElement x, ECFieldElement y, ECFieldElement[] zs,
            bool withCompression)
            : base(curve, x, y, zs, withCompression)
        {
        }

        protected override ECPoint Detach()
        {
            return new SecP192K1Point(null, AffineXCoord, AffineYCoord);
        }

        protected internal override bool CompressionYTilde
        {
            get { return this.AffineYCoord.TestBitZero(); }
        }

        public override ECPoint Add(ECPoint b)
        {
            if (this.IsInfinity)
                return b;
            if (b.IsInfinity)
                return this;
            if (this == b)
                return Twice();

            ECCurve curve = this.Curve;

            SecP192K1FieldElement X1 = (SecP192K1FieldElement)this.RawXCoord, Y1 = (SecP192K1FieldElement)this.RawYCoord;
            SecP192K1FieldElement X2 = (SecP192K1FieldElement)b.RawXCoord, Y2 = (SecP192K1FieldElement)b.RawYCoord;

            SecP192K1FieldElement Z1 = (SecP192K1FieldElement)this.RawZCoords[0];
            SecP192K1FieldElement Z2 = (SecP192K1FieldElement)b.RawZCoords[0];

            uint[] tt1 = Nat192.CreateExt();
            uint[] tt2 = Nat192.CreateExt();
            uint[] t3 = Nat192.Create();
            uint[] t4 = Nat192.Create();

            bool Z1IsOne = Z1.IsOne;
            uint[] U2, S2;
            if (Z1IsOne)
            {
                U2 = X2.x;
                S2 = Y2.x;
            }
            else
            {
                S2 = t3;
                SecP192K1Field.Square(Z1.x, S2);

                U2 = tt2;
                SecP192K1Field.Multiply(S2, X2.x, U2);

                SecP192K1Field.Multiply(S2, Z1.x, S2);
                SecP192K1Field.Multiply(S2, Y2.x, S2);
            }

            bool Z2IsOne = Z2.IsOne;
            uint[] U1, S1;
            if (Z2IsOne)
            {
                U1 = X1.x;
                S1 = Y1.x;
            }
            else
            {
                S1 = t4;
                SecP192K1Field.Square(Z2.x, S1);

                U1 = tt1;
                SecP192K1Field.Multiply(S1, X1.x, U1);

                SecP192K1Field.Multiply(S1, Z2.x, S1);
                SecP192K1Field.Multiply(S1, Y1.x, S1);
            }

            uint[] H = Nat192.Create();
            SecP192K1Field.Subtract(U1, U2, H);

            uint[] R = tt2;
            SecP192K1Field.Subtract(S1, S2, R);

            // Check if b == this or b == -this
            if (Nat192.IsZero(H))
            {
                if (Nat192.IsZero(R))
                {
                    // this == b, i.e. this must be doubled
                    return this.Twice();
                }

                // this == -b, i.e. the result is the point at infinity
                return curve.Infinity;
            }

            uint[] HSquared = t3;
            SecP192K1Field.Square(H, HSquared);

            uint[] G = Nat192.Create();
            SecP192K1Field.Multiply(HSquared, H, G);

            uint[] V = t3;
            SecP192K1Field.Multiply(HSquared, U1, V);

            Nat192.Mul(S1, G, tt1);

            SecP192K1FieldElement X3 = new SecP192K1FieldElement(t4);
            SecP192K1Field.Square(R, X3.x);
            SecP192K1Field.Add(X3.x, G, X3.x);
            SecP192K1Field.Subtract(X3.x, V, X3.x);
            SecP192K1Field.Subtract(X3.x, V, X3.x);

            SecP192K1FieldElement Y3 = new SecP192K1FieldElement(G);
            SecP192K1Field.Subtract(V, X3.x, Y3.x);
            Nat192.Mul(Y3.x, R, tt2);
            SecP192K1Field.SubtractExt(tt2, tt1, tt2);
            SecP192K1Field.Reduce(tt2, Y3.x);

            SecP192K1FieldElement Z3 = new SecP192K1FieldElement(H);
            if (!Z1IsOne)
            {
                SecP192K1Field.Multiply(Z3.x, Z1.x, Z3.x);
            }
            if (!Z2IsOne)
            {
                SecP192K1Field.Multiply(Z3.x, Z2.x, Z3.x);
            }

            ECFieldElement[] zs = new ECFieldElement[] { Z3 };

            return new SecP192K1Point(curve, X3, Y3, zs, IsCompressed);
        }

        public override ECPoint Twice()
        {
            if (this.IsInfinity)
                return this;

            ECCurve curve = this.Curve;

            SecP192K1FieldElement Y1 = (SecP192K1FieldElement)this.RawYCoord;
            if (Y1.IsZero)
                return curve.Infinity;

            SecP192K1FieldElement X1 = (SecP192K1FieldElement)this.RawXCoord, Z1 = (SecP192K1FieldElement)this.RawZCoords[0];

            uint[] Y1Squared = Nat192.Create();
            SecP192K1Field.Square(Y1.x, Y1Squared);

            uint[] T = Nat192.Create();
            SecP192K1Field.Square(Y1Squared, T);

            uint[] t1 = Nat192.Create();
            SecP192K1Field.Square(X1.x, t1);

            uint[] M = Nat192.Create();
            SecP192K1Field.Twice(t1, M);
            SecP192K1Field.Add(M, t1, M);

            uint[] S = Y1Squared;
            SecP192K1Field.Multiply(Y1Squared, X1.x, S);
            SecP192K1Field.Twice(S, S);
            SecP192K1Field.Twice(S, S);

            SecP192K1Field.Twice(T, t1);
            SecP192K1Field.Twice(t1, t1);
            SecP192K1Field.Twice(t1, t1);

            SecP192K1FieldElement X3 = new SecP192K1FieldElement(T);
            SecP192K1Field.Square(M, X3.x);
            SecP192K1Field.Subtract(X3.x, S, X3.x);
            SecP192K1Field.Subtract(X3.x, S, X3.x);

            SecP192K1FieldElement Y3 = new SecP192K1FieldElement(S);
            SecP192K1Field.Subtract(S, X3.x, Y3.x);
            SecP192K1Field.Multiply(Y3.x, M, Y3.x);
            SecP192K1Field.Subtract(Y3.x, t1, Y3.x);

            SecP192K1FieldElement Z3 = new SecP192K1FieldElement(M);
            SecP192K1Field.Twice(Y1.x, Z3.x);
            if (!Z1.IsOne)
            {
                SecP192K1Field.Multiply(Z3.x, Z1.x, Z3.x);
            }

            return new SecP192K1Point(curve, X3, Y3, new ECFieldElement[] { Z3 }, IsCompressed);
        }

        public override ECPoint TwicePlus(ECPoint b)
        {
            if (this == b)
                return ThreeTimes();
            if (this.IsInfinity)
                return b;
            if (b.IsInfinity)
                return Twice();

            ECFieldElement Y1 = this.RawYCoord;
            if (Y1.IsZero)
                return b;

            return Twice().Add(b);
        }

        public override ECPoint ThreeTimes()
        {
            if (this.IsInfinity || this.RawYCoord.IsZero)
                return this;

            // NOTE: Be careful about recursions between TwicePlus and ThreeTimes
            return Twice().Add(this);
        }

        public override ECPoint Subtract(ECPoint b)
        {
            if (b.IsInfinity)
                return this;

            return Add(b.Negate());
        }

        public override ECPoint Negate()
        {
            if (IsInfinity)
                return this;

            return new SecP192K1Point(Curve, RawXCoord, RawYCoord.Negate(), RawZCoords, IsCompressed);
        }
    }
}
