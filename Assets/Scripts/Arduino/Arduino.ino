#include "Arduino.h"
#include <Stepper.h>

Stepper stepp1(360, 12, 13);
Stepper stepp2(360, 10, 11);

int magnetPin = A0;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  stepp1.setSpeed(720);
  stepp2.setSpeed(720);
  pinMode(A0, OUTPUT);
}

void loop() {
  // put your main code here, to run repeatedly:
  String str = Serial.readString();
  if (str != "") {
    Serial.print(str);
    if (str == "RESET") {
      ResetSteppers();
    } else if (str == "ON") {
      magnet(true);
    } else if (str == "OFF") {
      magnet(false);
    } else {
      Serial.println(str);
      int index = str.indexOf(';');
      String subFirst = str.substring(0,index);
      String subLast = str.substring(index + 1,str.length());


      int stepperXVal = 0;
      int stepperYVal = 0;
      stepperXVal = subFirst.toFloat() * 1000 * 17; //Directions are capricious. set positive or negative if needed
      stepperYVal = subLast.toFloat() * 1000 * (-17);

      Serial.println("First");
      Serial.println(stepperXVal);
      Serial.println("Last");
      Serial.println(stepperYVal);

      StepperX(stepperXVal);
      StepperY(stepperYVal);
    }
  }
  delay(1000);
}

void stepper(Stepper stepp) {
  stepp.step(16000);
  Serial.println("STEPPER !!!");
  delay(1000);
  stepp.step(-16000);
  delay(1000);
}

void ResetSteppers() {
  Serial.println("RESET EN COURS");
   //Stucks in corner, a bit outside
  StepperX(300000);
  StepperY(-300000);
   //Put on default pos
}

void StepperX(int number) {
  stepp1.step(number);
}

void StepperY(int number) {
  stepp2.step(number);
}

void magnet(bool on) {
  if(on){
    digitalWrite(A0, HIGH);
  Serial.println("ON S'ACTIVE !!!");
  }
  else {
    analogWrite(A0, LOW);
  Serial.println("ON ARRETE TOUT !!!");
  }
  if (digitalRead(magnetPin) == HIGH) {
  Serial.println("allumed ");
  } else {
  Serial.println("etteint ");
  }
}
