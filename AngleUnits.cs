using static CalcItUWP.Utils;

namespace CalcItUWP {
	class AngleUnits {
		public static readonly AngleUnit DEGREE = new Degree();
		public static readonly AngleUnit RADIAN = new Radian();
		public static readonly AngleUnit GRAD = new Grad();

		public interface AngleUnit {
			double convertToRadians(double angle);
			double convertFromRadians(double angle);
		}

		class Degree: AngleUnit {
			public double convertToRadians(double angle) {
				return degToRad(angle);
			}

			public double convertFromRadians(double angle) {
				return radToDeg(angle);
			}
		}

		class Radian: AngleUnit {
			public double convertToRadians(double angle) {
				return angle;
			}

			public double convertFromRadians(double angle) {
				return angle;
			}
		}

		class Grad: AngleUnit {
			public double convertToRadians(double angle) {
				return gradToRad(angle);
			}

			public double convertFromRadians(double angle) {
				return radToGrad(angle);
			}
		}
	}
}
