#import "GoodiesDateTimePicker.h"
#import "GoodiesUtils.h"

#pragma clang diagnostic push
#pragma ide diagnostic ignored "OCUnusedGlobalDeclarationInspection"
GoodiesDateTimePicker *newPicker(void *callbackPtr, OnDateSelectedDelegate *onDateSelectedDelegate,
        void *cancelPtr, ActionVoidCallbackDelegate onCancel, int datePickerType) {
    return [[GoodiesDateTimePicker alloc] initWithCallbackPtr:callbackPtr
                                       onDateSelectedDelegate:onDateSelectedDelegate
                                                  onCancelPtr:cancelPtr
                                             onCancelDelegate:onCancel
                                               datePickerType:datePickerType];
}

extern "C" {

GoodiesDateTimePicker *pickerController;

void _showDatePickerWithInitialValue(
        int year, int month, int day, int hourOfDay, int minute,
        void *callbackPtr, OnDateSelectedDelegate *onDateSelectedDelegate,
        void *cancelPtr, ActionVoidCallbackDelegate onCancel, int datePickerType) {
    pickerController = nil;
    pickerController = newPicker(callbackPtr, onDateSelectedDelegate, cancelPtr, onCancel, datePickerType);
    [pickerController setInitialValuesWithYear:year
                                         month:month day:day hour:hourOfDay minute:minute];
    [pickerController showPicker];
}

void _showDatePicker(
        void *callbackPtr, OnDateSelectedDelegate *onDateSelectedDelegate,
        void *cancelPtr, ActionVoidCallbackDelegate onCancel, int datePickerType) {
    pickerController = nil;
    pickerController = newPicker(callbackPtr, onDateSelectedDelegate, cancelPtr, onCancel, datePickerType);
    [pickerController setInitialValueToNow];
    [pickerController showPicker];
}

void _showDatePickerWithRestrains(
        int year, int month, int day, int hourOfDay, int minute,
        void *callbackPtr, OnDateSelectedDelegate *onDateSelectedDelegate,
        void *cancelPtr, ActionVoidCallbackDelegate onCancel, int datePickerType,
        int minYear, int minMonth, int minDay, int minHourOfDay, int minMinute,
        int maxYear, int maxMonth, int maxDay, int maxHourOfDay, int maxMinute) {
    pickerController = nil;
    pickerController = newPicker(callbackPtr, onDateSelectedDelegate, cancelPtr, onCancel, datePickerType);
    [pickerController setInitialValuesWithYear:year
                                         month:month day:day hour:hourOfDay minute:minute];
    NSDate * minDate = [GoodiesUtils setDateFromYear:minYear month:minMonth day:minDay hour:minHourOfDay minute:minMinute];
    NSDate * maxDate = [GoodiesUtils setDateFromYear:maxYear month:maxMonth day:maxDay hour:maxHourOfDay minute:maxMinute];
    [pickerController showPickerWithMinDate:minDate MaxDate:maxDate];
}
}


#pragma clang diagnostic pop