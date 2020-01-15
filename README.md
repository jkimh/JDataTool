# JDataTool
데이터 세팅을 위한 툴
게임 데이터 파일을 수정하기 위한 편집 툴. 각 테이블의 칼럼에 대한 설명을 보며 무엇을 의미하는지 알 수 있습니다.


# 툴 데이터 추가 매뉴얼
A. 데이터 테이블 추가시 해야할 것 (툴 사용 시 기본 세팅)
  1. JDataTable.cs 에 각 항목 추가  
    - Constants 클래스에 항목 추가  
    - m_dataTableNameList 에 항목 추가  
    - GetDefaultDataString 에 기본 세팅 추가  
    - StringToTable 에 항목 추가  
    - TableToString 에 항목 추가  
  2. JItemData.cs 참고하여 데이터 프로퍼티 클래스 추가 (설명을 포함하여 메뉴얼을 대체한다.)  
   
B. 데이터 테이블 칼럼 추가 시 해야할 것
  1. JItemData.cs 에 해당하는 파일에 관련 변수와 프로퍼티 추가 (설명 및 기본값 포함)
  2. 같은 파일 내 Clone 함수 데이터 복사 로직 추가
