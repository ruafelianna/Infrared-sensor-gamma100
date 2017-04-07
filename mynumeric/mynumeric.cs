using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mynumeric
{
    public class GaussInterpolation
    {
        static double[,] makeMatrix(double[] x, double[] y)
        {
            int degree = x.Length;
	        var matr = new double[degree, degree+1];            
            for(int i=0; i<degree; ++i) {
    	        double s = x[i];
                double r = 1;
                matr[i,degree] = y[i];
                for(int j=0; j<degree; ++j) {
        	        matr[i,j] = r;
                    r*=s;
                }
            }
            return matr;
        }

        public static double[] calculate( double [] x, double [] y )
        {
            int degree = x.Length;
            System.Diagnostics.Debug.Assert(degree > 0 && degree == y.Length);
            var matr = makeMatrix( x, y);
	        var koefs = new double[degree];
            double s,r;
   	        for (int k=0; k<degree; ++k) {
    	        int k1 = k+1;
                s = matr[k,k];
     	        int j = k;
                for(int i=k1; i<degree; ++i) {
        	        r = matr[i,k];
                    if( System.Math.Abs(r)>System.Math.Abs(s) ) {
            	        s = r;
                        j = i;
                    }
                }
                if (j!=k) for (int i=k; i<degree+1; ++i) {
        	        r = matr[k,i];
         	        matr[k,i] = matr[j,i];
         	        matr[j,i] = r;
                }
                if (s==0) return koefs;

                for (int j_=k1; j_<degree+1; ++j_) matr[k,j_]/=s;
                for (int i = k1; i < degree; ++i) {
                    r = matr[i, k];
                    for (int j_ = k1; j_ < degree + 1; ++j_)
                        matr[i, j_] = matr[i, j_] - matr[k, j_] * r;
                }        
            }
            for (int i=degree-1; i>-1; --i) {
    	        s = matr[i,degree];
                for (int j = i+1; j<degree; ++j)
                    s = s - matr[i,j] * koefs[j];
                koefs[i] = s;
            }
            return koefs;
        }
    }
}
