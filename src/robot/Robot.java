package robot;

import com.ctre.phoenix.motorcontrol.ControlMode;
import com.ctre.phoenix.motorcontrol.can.TalonSRX;
import edu.wpi.first.wpilibj.GenericHID;
import edu.wpi.first.wpilibj.IterativeRobot;
import edu.wpi.first.wpilibj.XboxController;
import edu.wpi.first.wpilibj.smartdashboard.SmartDashboard;

public class Robot extends IterativeRobot {

    public double intakeSpeed;
    public XboxController xboxDrive;

    public static TalonSRX intakeLeft = new TalonSRX(Constants.PORT_INTAKE_LEFT);
    public static TalonSRX intakeRight = new TalonSRX(Constants.PORT_INTAKE_RIGHT);

    @Override
    public void robotInit() {
        xboxDrive = new XboxController(Constants.PORT_XBOX_DRIVE);
        intakeSpeed = 0.0;
        SmartDashboard.putNumber("Intake Speed", 0.0);
        intakeRight.follow(intakeLeft);

    }

    @Override
    public void disabledInit() { }


    @Override
    public void teleopInit() { }

    @Override
    public void testInit() { }


    @Override
    public void disabledPeriodic() { }


    @Override
    public void teleopPeriodic() {

        //Smart Dashboard

        intakeSpeed = SmartDashboard.getNumber("Intake Speed", 0.0);


        //Remote

        //intakeSpeed = xboxDrive.getTriggerAxis(XboxController.Hand.kRight);

        intakeLeft.set(ControlMode.PercentOutput, intakeSpeed);

    }

    @Override
    public void testPeriodic() { }
}