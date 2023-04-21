Helianthus
==========

![Helianthus](./Resources/helianthus-icon-192.png)

**Helianthus**: Crop-Centric Sunlight-Informed Tool for efficient urban agriculture design and planning

1\. Authors
-----------

- Alba Lombardia-Alonso
- Joel Parker
- Helianthus is currently developed at the Singapore University of Technology and Design (SUTD)

2\. Introducing Helianthus
-----------

Helianthus is the scientific name of sunflowers. These crops are known for their heliotropic movement to face the sun during the day. Helianthus will be a plugin for Grasshopper that performs a detailed analysis of light-informed data to produce customized, interactive visualizations for crop-centric urban agriculture design and planning.

Helianthus imports three main groups of light-informed data into Grasshopper: 
1. Standard environmental sunlight data from EnergyPlus Weather files (EPW).
2. Crop Light Requirements Data.
3. Materials Light Transmittance Data.

It will provide a variety of 2D interactive graphics that support decision-making during the early stages of design and planning city-integrated agriculture. Helianthus also will support the evaluation of sensorized sunlight data, crop yield and supplemental artificial lighting. Integrating the tool in a visual programming environment allows instantaneous feedback and a high degree of customization for the design and planning of future urban agriculture developments.

With Helianthus, it will be possible to:
1. Select urban surfaces for future agriculture in a digital model in Rhino.
2. Include a location to obtain site-specific sunlight values measured in DLIs through a conversion in Grasshopper.
3. Analyse the possibilities of urban agriculture in those locations.
4. Benchmark the sunlight data with empirical data.
5. Output the most suitable crops for those locations and their yield.
6. Calibrate the level of light received by the crops according to cover materials transmittances.
7. Calculate the supplemental artificial lighting needed for year-round production.
8. Calculate the solar energy needed for supplemental artificial lighting.
9. Analyse the results in real-time.
10. Provide interactive agriculture graphics that support decision-making.
11. Combine Helianthus with other plug-ins.
12. Participate in the development of Helianthus by including data. These capabilities make Helianthus perfect for integrating agricultural knowledge within architecture, urban planning, and product design.

It is composed of three categories:
1. Import
2. Analyse Data
3. Visualize Data

Inside these categories, five components have been elaborated and are described consecutively.
1. Crops list: Create a crop list from the Helianthus database.
2. DLI Suitability: Filters crops based on DLI thresholds.
3. Analyse the possibilities of urban agriculture in those locations.
4. DLI Visualization: Visualize site-specific crop-surface DLI suitability.
5. Legend: Fix legend parameters for visualizations.