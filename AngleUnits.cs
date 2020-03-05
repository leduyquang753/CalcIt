using static CalcItUWP.Utils;

namespace CalcItUWP {
	public class AngleUnits {
		public static readonly AngleUnit DEGREE = new Degree();
		public static readonly AngleUnit RADIAN = new Radian();
		public static readonly AngleUnit GRADIAN = new Gradian();

		public interface AngleUnit {
			double convertToRadians(double angle);
			double convertFromRadians(double angle);
		}

		public class Degree: AngleUnit {
			public double convertToRadians(double angle) {
				return degToRad(angle);
			}

			public double convertFromRadians(double angle) {
				return radToDeg(angle);
			}
		}

		public class Radian: AngleUnit {
			public double convertToRadians(double angle) {
				return angle;
			}

			public double convertFromRadians(double angle) {
				return angle;
			}
		}

		public class Gradian: AngleUnit {
			public double convertToRadians(double angle) {
				return gradToRad(angle);
			}

			public double convertFromRadians(double angle) {
				return radToGrad(angle);
			}
		}
	}
}
